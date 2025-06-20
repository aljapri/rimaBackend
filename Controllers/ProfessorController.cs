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

            var attendanceGroups = enrollments
                .GroupBy(e => new
                {
                    CourseId = e.ProfessorCourse.Course.Id,
                    CourseName = e.ProfessorCourse.Course.Name,
                    MaxAllowed = e.ProfessorCourse.Course.MaxAbsenceLimit
                })
                .Select(async group =>
                {
                    int totalAbsence = 0;

                    foreach (var e in group)
                    {
                        int count = await _context.Attendances
                            .Where(a => a.StudentId == studentId
                                        && a.ProfessorCourseId == e.ProfessorCourseId
                                        && !a.IsPresent)
                            .CountAsync();
                        totalAbsence += count;
                    }

                    string status = totalAbsence > group.Key.MaxAllowed
                        ? "ممنوع من التقديم"
                        : totalAbsence == group.Key.MaxAllowed
                            ? "تحذير: وصلت للحد المسموح"
                            : "منتظم";

                    return new
                    {
                        group.Key.CourseId,
                        group.Key.CourseName,
                        group.Key.MaxAllowed,
                        AbsenceCount = totalAbsence,
                        Status = status
                    };
                });

            var result = await Task.WhenAll(attendanceGroups);

            return Ok(result);
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

            var attendance = new Attendance
            {
                StudentId = dto.StudentId,
                ProfessorCourseId = dto.ProfessorCourseId,
                ProfessorCourse = professorCourse, // optional, EF will resolve from FK
                Course = professorCourse.Course,   // here we assign the Course
                IsPresent = dto.IsPresent,
                Notes = dto.Notes,
                SessionDate = DateTime.UtcNow
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




    }

    public class RecordAttendanceDto
    {
        public Guid StudentId { get; set; }
        public Guid ProfessorCourseId { get; set; }
        public bool IsPresent { get; set; }
        public string? Notes { get; set; }
    }


}
