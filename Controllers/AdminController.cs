using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using kalamon_University.DTOs.Admin;
using kalamon_University.DTOs.Auth;
using kalamon_University.DTOs.ProfessorPortal;
using kalamon_University.Interfaces;
using kalamon_University.Models.Entities;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using kalamon_University.DTOs.Course;
using kalamon_University.DTOs.Student;
using kalamon_University.Data;
using Microsoft.EntityFrameworkCore;

namespace kalamon_University.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    // [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IProfessorService _professorService;
        private readonly IAuthService _authService;
        private readonly ICourseService _courseService;
        private readonly IStudentService _studentService;
        private readonly AppDbContext _context;

        public AdminController(
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


        /// <summary>
        /// يقوم المسؤول بإنشاء حساب جديد لأستاذ (بريد وكلمة مرور وتخصص)
        /// </summary>
        [HttpPost("create-professor-account")]
        public async Task<IActionResult> CreateProfessorAccount([FromBody] RegisterDto dto)
        {
            // تأكد من أن الدور هو أستاذ
            dto.RoleName = "Professor";

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(dto);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// جلب كل الأساتذة
        /// </summary>
        [HttpGet("professors")]
        public async Task<IActionResult> GetAllProfessors()
        {
            var professors = await _professorService.GetAllAsync();

            var result = professors.Select(p => new ProfessorProfileDto
            {
                UserId = p.User.Id,
                FullName = p.User.FullName,
                Email = p.User.Email,
                UserName = p.User.UserName,
                EmailConfirmed = p.User.EmailConfirmed,
                Specialization = p.Specialization
            });

            return Ok(result);
        }

        /// <summary>
        /// جلب أستاذ حسب المعرّف
        /// </summary>
        [HttpGet("professors/{id}")]
        public async Task<IActionResult> GetProfessorById(Guid id)
        {
            var professor = await _professorService.GetByIdAsync(id);

            if (professor == null)
                return NotFound("الأستاذ غير موجود.");

            var result = new ProfessorProfileDto
            {
                UserId = professor.User.Id,
                FullName = professor.User.FullName,
                Email = professor.User.Email,
                UserName = professor.User.UserName,
                EmailConfirmed = professor.User.EmailConfirmed,
                Specialization = professor.Specialization
            };

            return Ok(result);
        }

        /// <summary>
        /// تحديث بيانات أستاذ
        /// </summary>
        [HttpPut("update-professor")]
        public async Task<IActionResult> UpdateProfessor([FromBody] ManageProfessorDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var professor = new Professor
            {
                UserId = dto.UserId,
                Specialization = dto.Specialization
            };

            await _professorService.UpdateAsync(professor);
            return NoContent(); // 204
        }

        /// <summary>
        /// حذف أستاذ
        /// </summary>
        [HttpDelete("delete-professor/{professorId}")]
        public async Task<IActionResult> DeleteProfessor(Guid professorId)
        {
            var result = await _professorService.DeleteAsync(professorId);
            if (!result)
                return NotFound("الأستاذ غير موجود أو لا يمكن حذفه.");

            return Ok("تم حذف الأستاذ بنجاح.");
        }

        [HttpPost("create-user-account")]
        public async Task<IActionResult> CreateUserAccount([FromBody] RegisterDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(dto);

            if (!result.Succeeded)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _studentService.GetAllAsync();

            var result = students.Select(s => new StudentProfileDtoAdmin(
                s.User.Id,
                s.User.FullName,
                s.User.Email,
                s.User.UserName,
                s.User.EmailConfirmed
            ));

            return Ok(result);
        }
        [HttpPut("update-student")]
        public async Task<IActionResult> UpdateStudent([FromBody] UpdateStudentDtoAdmin dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Fetch existing student (including User entity)
            var student = await _studentService.GetByIdAsync(dto.UserId);
            if (student == null)
                return NotFound("Student not found.");

            // Update the user properties
            student.User.FullName = dto.FullName;
            student.User.Email = dto.Email;
            student.User.UserName = dto.UserName;

            // Save the updated student (or user)
            await _studentService.UpdateAsync(student);

            return NoContent(); // 204 No Content - success with no data return
        }

        [HttpDelete("delete-student/{studentId}")]
        public async Task<IActionResult> DeleteStudent(Guid studentId)
        {
            var result = await _studentService.DeleteAsync(studentId);
            if (!result)
                return NotFound("الطالب غير موجود أو لا يمكن حذفه.");

            return Ok("تم حذف الطالب بنجاح.");
        }
        [HttpPost("add-course")]
        public async Task<IActionResult> AddCourse([FromBody] CreateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var course = new Course
            {
                Name = dto.Name,
                PracticalHours = dto.PracticalHours,
                TheoreticalHours = dto.TheoreticalHours,
                TotalHours = dto.TotalHours,
                MaxAbsenceLimit = dto.MaxAbsenceLimit
            };

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            return Ok(course);
        }
        [HttpPut("update-course/{id}")]
        public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound("Course not found.");

            course.Name = dto.Name;
            course.PracticalHours = dto.PracticalHours;
            course.TheoreticalHours = dto.TheoreticalHours;
            course.TotalHours = dto.TotalHours;
            course.MaxAbsenceLimit = dto.MaxAbsenceLimit;

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpDelete("delete-course/{id}")]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
                return NotFound("Course not found.");

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return Ok("Course deleted successfully.");
        }

        // تعيين أستاذ لكورس (ربط Many-to-Many)
        [HttpPost("assign-professor-to-course")]
        public async Task<IActionResult> AssignProfessorToCourse([FromBody] AssignProfessorToCourseDto dto)
        {
            var professor = await _context.Professors.FindAsync(dto.ProfessorId);
            var course = await _context.Courses.FindAsync(dto.CourseId);

            if (professor == null || course == null)
                return NotFound("Professor or Course not found.");

            var exists = await _context.ProfessorCourses
                .AnyAsync(pc => pc.ProfessorId == dto.ProfessorId && pc.CourseId == dto.CourseId);

            if (exists)
                return BadRequest("This professor is already assigned to the course.");

            var professorCourse = new ProfessorCourse
            {
                ProfessorId = dto.ProfessorId,
                CourseId = dto.CourseId
            };

            await _context.ProfessorCourses.AddAsync(professorCourse);
            await _context.SaveChangesAsync();

            return Ok("Professor assigned to course successfully.");
        }
        // جلب كل الكورسات مع بيانات الأساتذة المعينين (اختياري)
        [HttpGet("courses")]
        public async Task<IActionResult> GetAllCourses()
        {
            var courses = await _context.Courses
                .Include(c => c.ProfessorCourses)
                    .ThenInclude(pc => pc.Professor)
                        .ThenInclude(p => p.User)
                .ToListAsync();

            var result = courses.Select(c => new
            {
                c.Id,
                c.Name,
                c.PracticalHours,
                c.TheoreticalHours,
                c.TotalHours,
                c.MaxAbsenceLimit,
                Professors = c.ProfessorCourses.Select(pc => new
                {
                    pc.Professor.UserId,
                    pc.Professor.User.FullName,
                    pc.Professor.Specialization
                })
            });

            return Ok(result);
        }
        [HttpPost("assign-student-to-course")]
        public async Task<IActionResult> AssignStudentToCourse([FromBody] AssignStudentToCourseDto dto)
        {
            // تحقق من صحة البيانات
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // تحقق من وجود الطالب
            var student = await _context.Students.FindAsync(dto.StudentId);
            if (student == null)
                return NotFound("الطالب غير موجود.");

            // تحقق من وجود الكورس
            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null)
                return NotFound("الكورس غير موجود.");

            // تحقق إذا الطالب مسجل مسبقاً في هذا الكورس
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == dto.StudentId && e.CourseId == dto.CourseId);

            if (existingEnrollment != null)
                return BadRequest("الطالب مسجل بالفعل في هذا الكورس.");

            // أنشئ تسجيل الطالب في الكورس
            var enrollment = new Enrollment
            {
                StudentId = dto.StudentId,
                CourseId = dto.CourseId,
                EnrollmentDate = DateTime.UtcNow // إذا كان لديك هذا الحقل
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return Ok("تم تعيين الطالب إلى الكورس بنجاح.");
        }
        [HttpGet("course/{courseId}/students")]
        public async Task<IActionResult> GetStudentsByCourse(int courseId)
        {
            // تحقق من وجود الكورس
            var courseExists = await _context.Courses.AnyAsync(c => c.Id == courseId);
            if (!courseExists)
                return NotFound("الكورس غير موجود.");

            // جلب الطلاب المسجلين في الكورس مع بياناتهم الأساسية
            var students = await _context.Enrollments
                .Where(e => e.CourseId == courseId)
                .Include(e => e.Student)
                    .ThenInclude(s => s.User) // لجلب بيانات المستخدم المرتبط بالطالب
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
        [HttpGet("student/{studentId}/courses")]
        public async Task<IActionResult> GetCoursesForStudent(Guid studentId)
        {
            var student = await _context.Students
                .Include(s => s.Enrollments)
                    .ThenInclude(e => e.Course)
                .FirstOrDefaultAsync(s => s.UserId == studentId);

            if (student == null)
                return NotFound("الطالب غير موجود.");

            var courses = student.Enrollments.Select(e => new
            {
                CourseId = e.Course.Id,
                e.Course.Name,
                e.Course.PracticalHours,
                e.Course.TheoreticalHours,
                e.Course.TotalHours
            });

            return Ok(courses);
        }
        [HttpGet("professor/{professorId}/students")]
        public async Task<IActionResult> GetStudentsForProfessor(Guid professorId)
        {
            var professor = await _context.Professors
                .Include(p => p.ProfessorCourses)
                    .ThenInclude(pc => pc.Course)
                        .ThenInclude(c => c.Enrollments)
                            .ThenInclude(e => e.Student)
                                .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.UserId == professorId);

            if (professor == null)
                return NotFound("الأستاذ غير موجود.");

            var students = professor.ProfessorCourses
                .SelectMany(pc => pc.Course.Enrollments)
                .Select(e => new
                {
                    StudentId = e.Student.UserId,
                    e.Student.User.FullName,
                    e.Student.User.Email,
                    e.Student.User.UserName,
                    CourseName = e.Course.Name
                })
                .Distinct()
                .ToList();

            return Ok(students);
        }






    }


    // DTO لتحديث الأستاذ (اختياري يمكن وضعه في ملف مستقل)
    public class ManageProfessorDto
    {
        public Guid UserId { get; set; }
        public string Specialization { get; set; } = "غير محدد";
    }
    public class CreateCourseDto
    {
        public string Name { get; set; } = string.Empty;
        public int PracticalHours { get; set; }
        public int TheoreticalHours { get; set; }
        public int TotalHours { get; set; }
        public int MaxAbsenceLimit { get; set; }
    }

    public class UpdateCourseDto : CreateCourseDto { }

    public class AssignProfessorToCourseDto
    {
        public Guid ProfessorId { get; set; }
        public int CourseId { get; set; }
    }
    public class AssignStudentToCourseDto
    {
        public Guid StudentId { get; set; }
        public int CourseId { get; set; }
    }
}
