using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kalamon_University.Controllers;
using kalamon_University.Data;
using kalamon_University.Interfaces;
using kalamon_University.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfessorController : ControllerBase
    {
        private readonly IProfessorService _professorService;
        private readonly IAuthService _authService;
        private readonly ICourseService _courseService;
        private readonly IStudentService _studentService;
        private readonly AppDbContext _context;

        public ProfessorController(
            IProfessorService professorService,
            IAuthService authService,
            ICourseService courseService,
            IStudentService studentService,
            AppDbContext context
            )
        {
            _professorService = professorService;
            _authService = authService;
            _courseService = courseService;
            _studentService = studentService;
            _context = context;
        }

        [HttpPost("assign-student-to-professor-course")]
        public async Task<IActionResult> AssignStudentToProfessorCourse([FromBody] AssignStudentToProfessorCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null)
                return NotFound("الطالب غير موجود.");

            var professorCourse = await _context.ProfessorCourses
                .FindAsync(dto.ProfessorCourseId);

            if (professorCourse == null)
                return NotFound("الكورس للأستاذ غير موجود.");

            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == dto.StudentId && e.ProfessorCourseId == dto.ProfessorCourseId);

            if (alreadyEnrolled)
                return BadRequest("الطالب مسجل بالفعل في هذا الكورس.");

            var enrollment = new Enrollment
            {
                StudentId = dto.StudentId,
                ProfessorCourseId = dto.ProfessorCourseId,
                EnrollmentDate = DateTime.UtcNow
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok("تم تسجيل الطالب بنجاح.");
        }
        [HttpDelete("unenroll-student")]
        public async Task<IActionResult> UnenrollStudentFromCourse([FromQuery] Guid studentId, [FromQuery] Guid professorCourseId)
        {
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.ProfessorCourseId == professorCourseId);

            if (enrollment == null)
                return NotFound("التسجيل غير موجود.");

            _context.Enrollments.Remove(enrollment);
            await _context.SaveChangesAsync();

            return Ok("تم حذف الطالب من الكورس بنجاح.");
        }


        [HttpGet("professor/{professorId}/students")]
        public async Task<IActionResult> GetStudentsByProfessor(Guid professorId)
        {
            // Get all professorCourseIds for this professor
            var professorCourseIds = await _context.ProfessorCourses
                .Where(pc => pc.ProfessorId == professorId)
                .Select(pc => pc.Id)
                .ToListAsync();

            if (!professorCourseIds.Any())
                return NotFound("هذا الأستاذ ليس لديه أي كورسات.");

            // Get all enrollments for those professor courses including student user info
            var students = await _context.Enrollments
                .Where(e => professorCourseIds.Contains(e.ProfessorCourseId))
                .Include(e => e.Student)
                    .ThenInclude(s => s.User) // include User entity for student details
                .Select(e => new
                {
                    StudentId = e.StudentId,
                    FullName = e.Student.User.FullName,
                    Email = e.Student.User.Email,
                    UserName = e.Student.User.UserName
                })
                .Distinct()
                .ToListAsync();

            if (!students.Any())
                return NotFound("لا يوجد طلاب مسجلين مع هذا الأستاذ.");

            return Ok(students);
        }




        [HttpGet("professor/{professorId}/courses")]
        public async Task<IActionResult> GetCoursesByProfessor(Guid professorId)
        {
            var courses = await _context.ProfessorCourses
                .Where(pc => pc.ProfessorId == professorId)
                .Include(pc => pc.Course)
                .Select(pc => new
                {
                    CourseId = pc.Course.Id,
                    CourseName = pc.Course.Name,
                    PracticalHours = pc.Course.PracticalHours,
                    TheoreticalHours = pc.Course.TheoreticalHours,
                    TotalHours = pc.Course.TotalHours,
                    MaxAbsenceLimit = pc.Course.MaxAbsenceLimit
                })
                .ToListAsync();

            if (!courses.Any())
                return NotFound("لا توجد كورسات لهذا الأستاذ.");

            return Ok(courses);
        }










        [HttpGet("Student/{studentId}/attendance-summary")]
        public async Task<IActionResult> GetAttendanceSummary(Guid studentId)
        {
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == studentId);

            if (student == null)
                return NotFound("الطالب غير موجود.");

            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.ProfessorCourse)
                    .ThenInclude(pc => pc.Course)
                .ToListAsync();

            if (!enrollments.Any())
                return NotFound("الطالب غير مسجل في أي كورس.");

            var professorCourseIds = enrollments.Select(e => e.ProfessorCourseId).ToList();

            var absences = await _context.Attendances
                .Where(a => a.StudentId == studentId && professorCourseIds.Contains(a.ProfessorCourseId) && !a.IsPresent)
                .ToListAsync();

            var groupedByCourse = enrollments.GroupBy(e => e.ProfessorCourse.Course).ToList();

            var results = new List<object>();

            foreach (var group in groupedByCourse)
            {
                var course = group.Key;
                int fullAttendance = course.fullAttendance;

                int totalPracticalAbsence = 0;
                int totalTheoreticalAbsence = 0;
                List<string> courseParts = new();

                foreach (var enrollment in group)
                {
                    var pc = enrollment.ProfessorCourse;
                    var absencesForPc = absences.Where(a => a.ProfessorCourseId == pc.Id).ToList();

                    if (pc.Practical && !pc.Theoretical)
                    {
                        int count = absencesForPc.Count;
                        totalPracticalAbsence += count;
                        double percent = fullAttendance > 0 ? (double)count / fullAttendance : 0;
                        courseParts.Add($"{course.Name} (عملي): {percent:P1}");
                    }
                    else if (!pc.Practical && pc.Theoretical)
                    {
                        int count = absencesForPc.Count;
                        totalTheoreticalAbsence += count;
                        double percent = fullAttendance > 0 ? (double)count / fullAttendance : 0;
                        courseParts.Add($"{course.Name} (نظري): {percent:P1}");
                    }
                    else
                    {
                        int practical = absencesForPc.Count(a => a.Notes?.Contains("عملي") ?? false);
                        int theoretical = absencesForPc.Count(a => a.Notes?.Contains("نظري") ?? false);
                        totalPracticalAbsence += practical;
                        totalTheoreticalAbsence += theoretical;

                        double practicalPercent = fullAttendance > 0 ? (double)practical / fullAttendance : 0;
                        double theoreticalPercent = fullAttendance > 0 ? (double)theoretical / fullAttendance : 0;

                        courseParts.Add($"{course.Name} (عملي): {practicalPercent:P1}");
                        courseParts.Add($"{course.Name} (نظري): {theoreticalPercent:P1}");
                    }
                }

                double practicalAbsencePercentage = fullAttendance > 0 ? (double)totalPracticalAbsence / fullAttendance : 0;
                double theoreticalAbsencePercentage = fullAttendance > 0 ? (double)totalTheoreticalAbsence / fullAttendance : 0;
                double totalAbsencePercentage = practicalAbsencePercentage + theoreticalAbsencePercentage;

                double allowedPracticalPercentage = course.MaxAbsenceLimitPractical / 100.0;
                double allowedTheoreticalPercentage = course.MaxAbsenceLimitTheoretical / 100.0;
                double allowedTotalPercentage = allowedPracticalPercentage + allowedTheoreticalPercentage;

                string status = totalAbsencePercentage > allowedTotalPercentage ? "محروم"
                               : totalAbsencePercentage == allowedTotalPercentage ? "تحذير"
                               : "منتظم";

                results.Add(new
                {
                    courseId = course.Id,
                    course.Name,
                    course.fullAttendance,
                    course.PracticalHours,
                    course.TheoreticalHours,
                    course.MaxAbsenceLimitPractical,
                    course.MaxAbsenceLimitTheoretical,

                    practicalAbsenceCount = totalPracticalAbsence,
                    theoreticalAbsenceCount = totalTheoreticalAbsence,
                    totalAbsences = totalPracticalAbsence + totalTheoreticalAbsence,

                    practicalAbsencePercentage = $"{practicalAbsencePercentage:P1}",
                    theoreticalAbsencePercentage = $"{theoreticalAbsencePercentage:P1}",
                    totalAbsencePercentage = $"{totalAbsencePercentage:P1}",

                    allowedPracticalPercentage = $"{allowedPracticalPercentage:P1}",
                    allowedTheoreticalPercentage = $"{allowedTheoreticalPercentage:P1}",
                    allowedTotalPercentage = $"{allowedTotalPercentage:P1}",

                    status,
                    attendanceDetails = courseParts
                });
            }

            return Ok(results);
        }
















        [HttpGet("professor-course/{professorCourseId}/students")]
        public async Task<IActionResult> GetStudentsByProfessorCourseId(Guid professorCourseId)
        {
            var professorCourseExists = await _context.ProfessorCourses.AnyAsync(pc => pc.Id == professorCourseId);
            if (!professorCourseExists)
                return NotFound("الكورس غير موجود.");

            var students = await _context.Enrollments
                .Where(e => e.ProfessorCourseId == professorCourseId)
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .Select(e => new
                {
                    StudentId = e.StudentId,
                    FullName = e.Student.User.FullName,
                    Email = e.Student.User.Email,
                    UserName = e.Student.User.UserName
                })
                .ToListAsync();

            return Ok(students);
        }
        [HttpDelete("attendance/delete-latest")]
        public async Task<IActionResult> DeleteLatestAbsence([FromQuery] Guid studentId, [FromQuery] Guid professorCourseId)
        {
            var latestAbsence = await _context.Attendances
                .Where(a => a.StudentId == studentId && a.ProfessorCourseId == professorCourseId && !a.IsPresent)
                .OrderByDescending(a => a.SessionDate)
                .FirstOrDefaultAsync();

            if (latestAbsence == null)
                return NotFound("لا توجد غيابات لهذا الطالب.");

            _context.Attendances.Remove(latestAbsence);
            await _context.SaveChangesAsync();

            return Ok("تم حذف آخر غياب.");
        }
        [HttpDelete("attendance/delete-by-date")]
        public async Task<IActionResult> DeleteAttendanceByDate(
    [FromQuery] Guid studentId,
    [FromQuery] Guid professorCourseId,
    [FromQuery] DateTime sessionDate)
        {
            // Find attendance record for specific date
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.StudentId == studentId
                                       && a.ProfessorCourseId == professorCourseId
                                       && a.SessionDate.Date == sessionDate.Date);

            if (attendance == null)
                return NotFound("لا توجد سجلات حضور لهذا التاريخ.");

            _context.Attendances.Remove(attendance);
            await _context.SaveChangesAsync();

            return Ok("تم حذف سجل الحضور بنجاح.");
        }



        [HttpPost("record-attendance")]
        public async Task<IActionResult> RecordAttendance([FromBody] RecordAttendanceDto dto)
        {
            var enrollmentExists = await _context.Enrollments
                .AnyAsync(e => e.StudentId == dto.StudentId && e.ProfessorCourseId == dto.ProfessorCourseId);

            if (!enrollmentExists)
                return BadRequest("الطالب غير مسجل في هذا الكورس.");

            var professorCourse = await _context.ProfessorCourses
                .Include(pc => pc.Course)
                .FirstOrDefaultAsync(pc => pc.Id == dto.ProfessorCourseId);

            if (professorCourse == null)
                return NotFound("الكورس غير موجود.");

            // ✅ Compare only the date part
            bool attendanceExists = await _context.Attendances.AnyAsync(a =>
                a.StudentId == dto.StudentId &&
                a.ProfessorCourseId == dto.ProfessorCourseId &&
                a.SessionDate.Date == dto.SessionDate.Date);

            if (attendanceExists)
                return Conflict("تم تسجيل الحضور بالفعل لهذا الطالب في هذا التاريخ.");

            var attendance = new Attendance
            {
                StudentId = dto.StudentId,
                ProfessorCourseId = dto.ProfessorCourseId,
                ProfessorCourse = professorCourse,
                Course = professorCourse.Course,
                IsPresent = dto.IsPresent,
                Notes = dto.Notes,
                SessionDate = dto.SessionDate.Date // ✅ Truncate time
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();

            return Ok("تم تسجيل الغياب/الحضور بنجاح.");
        }




        [HttpGet("professor-course/{professorCourseId}/students-attendance")]
        public async Task<IActionResult> GetStudentsWithAbsenceCount(Guid professorCourseId)
        {
            var professorCourse = await _context.ProfessorCourses
                .Include(pc => pc.Course)
                .FirstOrDefaultAsync(pc => pc.Id == professorCourseId);

            Console.WriteLine(professorCourse);
            if (professorCourse == null)
                return NotFound("الكورس غير موجود.");

            var course = professorCourse.Course;
            if (course == null)
                return NotFound("المادة المرتبطة بالكورس غير موجودة.");

            var enrollments = await _context.Enrollments
                .Where(e => e.ProfessorCourseId == professorCourseId)
                .Include(e => e.Student)
                    .ThenInclude(s => s.User)
                .ToListAsync();

            var result = new List<object>();

            foreach (var enrollment in enrollments)
            {
                var absenceCount = await _context.Attendances
                    .Where(a => a.StudentId == enrollment.StudentId &&
                                a.ProfessorCourseId == professorCourseId &&
                                !a.IsPresent)
                    .CountAsync();

                var status = absenceCount > course.MaxAbsenceLimit
                    ? "ممنوع من التقديم"
                    : (absenceCount == course.MaxAbsenceLimit
                        ? "تحذير: وصلت للحد المسموح"
                        : "مسموح");

                result.Add(new
                {
                    enrollment.StudentId,
                    FullName = enrollment.Student.User.FullName,
                    AbsenceCount = absenceCount,
                    MaxAllowed = course.MaxAbsenceLimit,
                    Status = status
                });
            }

            return Ok(result);
        }
        [HttpGet("professor-course/{professorCourseId}")]
        public async Task<IActionResult> GetProfessorCourseDetails(Guid professorCourseId)
        {
            var professorCourse = await _context.ProfessorCourses
                .Include(pc => pc.Course)
                .FirstOrDefaultAsync(pc => pc.Id == professorCourseId);

            if (professorCourse == null)
                return NotFound("الكورس للأستاذ غير موجود.");

            return Ok(new
            {
                professorCourse.Id,
                professorCourse.ProfessorId,
                CourseId = professorCourse.Course.Id,
                CourseName = professorCourse.Course.Name,
                Practical = professorCourse.Practical,
                Theoretical = professorCourse.Theoretical,
                CourseFullAttendance = professorCourse.Course.fullAttendance,
                MaxAbsenceLimitPractical = professorCourse.Course.MaxAbsenceLimitPractical,
                MaxAbsenceLimitTheoretical = professorCourse.Course.MaxAbsenceLimitTheoretical,
                // أضف أي خصائص أخرى تريدها
            });
        }

        [HttpGet("course/{courseId}/absence-violators")]
        public async Task<IActionResult> GetAbsenceViolatorsForCourses(int courseId)
        {
            var professorCourses = await _context.ProfessorCourses
                .Where(pc => pc.CourseId == courseId)
                .Include(pc => pc.Course)
                .ToListAsync();

            if (!professorCourses.Any())
                return NotFound("لا يوجد كورسات مرتبطة بهذا المقرر.");

            var course = professorCourses.First().Course;
            int fullAttendance = course.fullAttendance;

            // النسب المسموح بها كنسبة عشرية (مثلاً 7% = 0.07)
            double allowedRatioPractical = course.MaxAbsenceLimitPractical / 100.0;
            double allowedRatioTheoretical = course.MaxAbsenceLimitTheoretical / 100.0;
            double totalAllowedRatio = allowedRatioPractical + allowedRatioTheoretical;

            var enrollments = await _context.Enrollments
                .Where(e => professorCourses.Select(pc => pc.Id).Contains(e.ProfessorCourseId))
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.ProfessorCourse)
                .ToListAsync();

            var grouped = enrollments
                .GroupBy(e => new { e.StudentId, e.Student.User.FullName })
                .ToList();

            var results = new List<object>();

            foreach (var group in grouped)
            {
                int totalPracticalAbsence = 0;
                int totalTheoreticalAbsence = 0;
                List<string> courseParts = new();

                foreach (var e in group)
                {
                    var pc = e.ProfessorCourse;
                    var absences = await _context.Attendances
                        .Where(a => a.StudentId == e.StudentId &&
                                    a.ProfessorCourseId == pc.Id &&
                                    !a.IsPresent)
                        .ToListAsync();

                    if (pc.Practical && !pc.Theoretical)
                    {
                        int practicalCount = absences.Count;
                        totalPracticalAbsence += practicalCount;
                        double practicalRatio = fullAttendance > 0 ? (double)practicalCount / fullAttendance : 0;
                        courseParts.Add($"{course.Name} (عملي): {practicalRatio:P1}");
                    }
                    else if (!pc.Practical && pc.Theoretical)
                    {
                        int theoreticalCount = absences.Count;
                        totalTheoreticalAbsence += theoreticalCount;
                        double theoreticalRatio = fullAttendance > 0 ? (double)theoreticalCount / fullAttendance : 0;
                        courseParts.Add($"{course.Name} (نظري): {theoreticalRatio:P1}");
                    }
                    else
                    {
                        int practical = absences.Count(a => a.Notes?.Contains("عملي") ?? false);
                        int theoretical = absences.Count(a => a.Notes?.Contains("نظري") ?? false);
                        totalPracticalAbsence += practical;
                        totalTheoreticalAbsence += theoretical;

                        double practicalRatio = fullAttendance > 0 ? (double)practical / fullAttendance : 0;
                        double theoreticalRatio = fullAttendance > 0 ? (double)theoretical / fullAttendance : 0;

                        courseParts.Add($"{course.Name} (نظري: {theoreticalRatio:P1} + عملي: {practicalRatio:P1})");
                    }
                }

                int totalAbsencesCount = totalPracticalAbsence + totalTheoreticalAbsence;

                double practicalAbsenceRatio = fullAttendance > 0 ? (double)totalPracticalAbsence / fullAttendance : 0;
                double theoreticalAbsenceRatio = fullAttendance > 0 ? (double)totalTheoreticalAbsence / fullAttendance : 0;
                double totalAbsenceRatio = practicalAbsenceRatio + theoreticalAbsenceRatio;

                string status = totalAbsenceRatio > totalAllowedRatio ? "محروم"
                                : totalAbsenceRatio == totalAllowedRatio ? "تحذير"
                                : "منتظم";

                if (status == "محروم")
                {
                    results.Add(new
                    {
                        group.Key.StudentId,
                        group.Key.FullName,

                        PracticalAbsenceCount = totalPracticalAbsence,
                        TheoreticalAbsenceCount = totalTheoreticalAbsence,
                        TotalAbsencesCount = totalAbsencesCount,

                        PracticalAbsencePercentage = $"{practicalAbsenceRatio:P1}",
                        TheoreticalAbsencePercentage = $"{theoreticalAbsenceRatio:P1}",
                        TotalAbsencePercentage = $"{totalAbsenceRatio:P1}",

                        AllowedPracticalPercentage = $"{allowedRatioPractical:P1}",
                        AllowedTheoreticalPercentage = $"{allowedRatioTheoretical:P1}",
                        AllowedTotalPercentage = $"{totalAllowedRatio:P1}",

                        Status = status,
                        courses = courseParts
                    });
                }
            }

            return Ok(results);
        }

        [HttpGet("professor/{professorId}/students-absence-summary")]
        public async Task<IActionResult> GetAbsenceSummaryForProfessor(Guid professorId)
        {
            var professorCourses = await _context.ProfessorCourses
                .Where(pc => pc.ProfessorId == professorId)
                .Include(pc => pc.Course)
                .ToListAsync();

            if (!professorCourses.Any())
                return NotFound("لا توجد كورسات لهذا الأستاذ.");

            var courseIds = professorCourses.Select(pc => pc.CourseId).Distinct().ToList();

            // Get all ProfessorCourse instances across all professors for same courses
            var relatedProfessorCourses = await _context.ProfessorCourses
                .Where(pc => courseIds.Contains(pc.CourseId))
                .Include(pc => pc.Course)
                .ToListAsync();

            var relatedProfessorCourseIds = relatedProfessorCourses.Select(pc => pc.Id).ToList();

            var enrollments = await _context.Enrollments
                .Where(e => relatedProfessorCourseIds.Contains(e.ProfessorCourseId))
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.ProfessorCourse)
                    .ThenInclude(pc => pc.Course)
                .ToListAsync();

            var grouped = enrollments
                .GroupBy(e => new { e.StudentId, e.Student.User.FullName })
                .ToList();

            var results = new List<object>();

            foreach (var studentGroup in grouped)
            {
                var courses = studentGroup
                    .GroupBy(e => e.ProfessorCourse.Course)
                    .ToList();

                foreach (var courseGroup in courses)
                {
                    var course = courseGroup.Key;
                    int fullAttendance = course.fullAttendance;
                    int totalPracticalAbsence = 0;
                    int totalTheoreticalAbsence = 0;

                    List<string> coursePartDetails = new();

                    foreach (var enrollment in courseGroup)
                    {
                        var pc = enrollment.ProfessorCourse;

                        var absences = await _context.Attendances
                            .Where(a => a.StudentId == enrollment.StudentId &&
                                        a.ProfessorCourseId == pc.Id &&
                                        !a.IsPresent)
                            .ToListAsync();

                        if (pc.Practical && !pc.Theoretical)
                        {
                            int count = absences.Count;
                            totalPracticalAbsence += count;
                            coursePartDetails.Add($"{course.Name} (عملي): {(double)count / fullAttendance:P1}");
                        }
                        else if (!pc.Practical && pc.Theoretical)
                        {
                            int count = absences.Count;
                            totalTheoreticalAbsence += count;
                            coursePartDetails.Add($"{course.Name} (نظري): {(double)count / fullAttendance:P1}");
                        }
                        else
                        {
                            int practical = absences.Count(a => a.Notes?.Contains("عملي") ?? false);
                            int theoretical = absences.Count(a => a.Notes?.Contains("نظري") ?? false);
                            totalPracticalAbsence += practical;
                            totalTheoreticalAbsence += theoretical;

                            coursePartDetails.Add($"{course.Name} (عملي): {(double)practical / fullAttendance:P1}");
                            coursePartDetails.Add($"{course.Name} (نظري): {(double)theoretical / fullAttendance:P1}");
                        }
                    }

                    double practicalAbsenceRatio = fullAttendance > 0 ? (double)totalPracticalAbsence / fullAttendance : 0;
                    double theoreticalAbsenceRatio = fullAttendance > 0 ? (double)totalTheoreticalAbsence / fullAttendance : 0;
                    double totalAbsenceRatio = practicalAbsenceRatio + theoreticalAbsenceRatio;

                    double allowedRatioPractical = course.MaxAbsenceLimitPractical / 100.0;
                    double allowedRatioTheoretical = course.MaxAbsenceLimitTheoretical / 100.0;
                    double allowedTotalRatio = allowedRatioPractical + allowedRatioTheoretical;

                    string status = totalAbsenceRatio > allowedTotalRatio ? "محروم"
                                   : totalAbsenceRatio == allowedTotalRatio ? "تحذير"
                                   : "منتظم";

                    results.Add(new
                    {
                        studentGroup.Key.StudentId,
                        studentGroup.Key.FullName,
                        courseId = course.Id,
                        courseName = course.Name,
                        PracticalAbsences = totalPracticalAbsence,
                        TheoreticalAbsences = totalTheoreticalAbsence,
                        TotalAbsences = totalPracticalAbsence + totalTheoreticalAbsence,
                        PracticalAbsencePercentage = $"{practicalAbsenceRatio:P1}",
                        TheoreticalAbsencePercentage = $"{theoreticalAbsenceRatio:P1}",
                        TotalAbsencePercentage = $"{totalAbsenceRatio:P1}",
                        AllowedPractical = $"{allowedRatioPractical:P1}",
                        AllowedTheoretical = $"{allowedRatioTheoretical:P1}",
                        AllowedTotal = $"{allowedTotalRatio:P1}",
                        Status = status,
                        Details = coursePartDetails
                    });
                }
            }

            return Ok(results);
        }

        [HttpGet("course/{courseId}/students-full-summary")]
public async Task<IActionResult> GetFullStudentSummaryForCourse(int courseId)
{
    var professorCourses = await _context.ProfessorCourses
        .Where(pc => pc.CourseId == courseId)
        .Include(pc => pc.Course)
        .ToListAsync();

    if (!professorCourses.Any())
        return NotFound("لا يوجد كورسات مرتبطة بهذا المقرر.");

    var course = professorCourses.First().Course;
    int fullAttendance = course.fullAttendance;

    double allowedRatioPractical = course.MaxAbsenceLimitPractical / 100.0;
    double allowedRatioTheoretical = course.MaxAbsenceLimitTheoretical / 100.0;
    double allowedTotalRatio = allowedRatioPractical + allowedRatioTheoretical;

    var enrollments = await _context.Enrollments
        .Where(e => professorCourses.Select(pc => pc.Id).Contains(e.ProfessorCourseId))
        .Include(e => e.Student).ThenInclude(s => s.User)
        .Include(e => e.ProfessorCourse)
        .ToListAsync();

    var groupedByStudent = enrollments
        .GroupBy(e => new { e.StudentId, e.Student.User.FullName, e.Student.User.Email, e.Student.User.UserName })
        .ToList();

    var results = new List<object>();

    foreach (var studentGroup in groupedByStudent)
    {
        int totalPracticalAbsence = 0;
        int totalTheoreticalAbsence = 0;
        List<string> details = new();
        List<string> practicalDates = new();
        List<string> theoreticalDates = new();
        List<object> attendanceEntries = new();

        foreach (var enrollment in studentGroup)
        {
            var pc = enrollment.ProfessorCourse;

            var absences = await _context.Attendances
                .Where(a => a.StudentId == enrollment.StudentId &&
                            a.ProfessorCourseId == pc.Id &&
                            !a.IsPresent)
                .ToListAsync();

            foreach (var absence in absences)
            {
                string type = "";

                if (pc.Practical && !pc.Theoretical)
                    type = "عملي";
                else if (!pc.Practical && pc.Theoretical)
                    type = "نظري";
                else if (absence.Notes?.Contains("عملي") == true)
                    type = "عملي";
                else if (absence.Notes?.Contains("نظري") == true)
                    type = "نظري";

                attendanceEntries.Add(new
                {
                    Date = absence.SessionDate.ToString("yyyy-MM-dd"),
                    Type = type,
                    ProfessorCourseId = pc.Id
                });
            }

            if (pc.Practical && !pc.Theoretical)
            {
                int count = absences.Count;
                totalPracticalAbsence += count;
                practicalDates.AddRange(absences.Select(a => a.SessionDate.ToString("yyyy-MM-dd")));
                details.Add($"عملي: {(double)count / fullAttendance:P1}");
            }
            else if (!pc.Practical && pc.Theoretical)
            {
                int count = absences.Count;
                totalTheoreticalAbsence += count;
                theoreticalDates.AddRange(absences.Select(a => a.SessionDate.ToString("yyyy-MM-dd")));
                details.Add($"نظري: {(double)count / fullAttendance:P1}");
            }
            else
            {
                var practical = absences.Where(a => a.Notes?.Contains("عملي") ?? false).ToList();
                var theoretical = absences.Where(a => a.Notes?.Contains("نظري") ?? false).ToList();

                totalPracticalAbsence += practical.Count;
                totalTheoreticalAbsence += theoretical.Count;

                practicalDates.AddRange(practical.Select(a => a.SessionDate.ToString("yyyy-MM-dd")));
                theoreticalDates.AddRange(theoretical.Select(a => a.SessionDate.ToString("yyyy-MM-dd")));

                details.Add($"نظري: {(double)theoretical.Count / fullAttendance:P1} - عملي: {(double)practical.Count / fullAttendance:P1}");
            }
        }

        double practicalRatio = fullAttendance > 0 ? (double)totalPracticalAbsence / fullAttendance : 0;
        double theoreticalRatio = fullAttendance > 0 ? (double)totalTheoreticalAbsence / fullAttendance : 0;
        double totalRatio = practicalRatio + theoreticalRatio;

        string status = totalRatio > allowedTotalRatio ? "محروم"
                      : totalRatio == allowedTotalRatio ? "تحذير"
                      : "منتظم";

        results.Add(new
        {
            studentGroup.Key.StudentId,
            FullName = studentGroup.Key.FullName,
            Email = studentGroup.Key.Email,
            UserName = studentGroup.Key.UserName,

            ProfessorCourseIds = studentGroup.Select(e => e.ProfessorCourseId).Distinct().ToList(),

            PracticalAbsences = totalPracticalAbsence,
            TheoreticalAbsences = totalTheoreticalAbsence,
            TotalAbsences = totalPracticalAbsence + totalTheoreticalAbsence,

            PracticalAbsenceDates = practicalDates,
            TheoreticalAbsenceDates = theoreticalDates,

            PracticalAbsencePercentage = $"{practicalRatio:P1}",
            TheoreticalAbsencePercentage = $"{theoreticalRatio:P1}",
            TotalAbsencePercentage = $"{totalRatio:P1}",

            AllowedPractical = $"{allowedRatioPractical:P1}",
            AllowedTheoretical = $"{allowedRatioTheoretical:P1}",
            AllowedTotal = $"{allowedTotalRatio:P1}",

            Status = status,
            CourseDetails = new
            {
                course.Id,
                course.Name,
                course.PracticalHours,
                course.TheoreticalHours,
                course.fullAttendance,
                course.MaxAbsenceLimitPractical,
                course.MaxAbsenceLimitTheoretical
            },
            Details = details,

            Attendances = attendanceEntries // ✅ NEW: all individual attendance records with ProfessorCourseId
        });
    }

    return Ok(results);
}















    }

    public class RecordAttendanceDto
    {
        public Guid StudentId { get; set; }
        public Guid ProfessorCourseId { get; set; }
        public bool IsPresent { get; set; }
        public string? Notes { get; set; }
        public DateTime SessionDate { get; set; } // <- Added this
    }



}

/*

*/