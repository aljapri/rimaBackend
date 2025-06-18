using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kalamon_University.Models.Entities
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 10)]
        public int PracticalHours { get; set; }

        [Range(0, 10)]
        public int TheoreticalHours { get; set; }

        [Range(0, 20)]
        public int TotalHours { get; set; }

        [Range(1, 20)]
        public int MaxAbsenceLimit { get; set; } = 5;

        // Calculated property
        [NotMapped]
        public int CalculatedTotalHours => PracticalHours + TheoreticalHours;

        // 🔁 Relationships
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Warning> Warnings { get; set; } = new List<Warning>();

        // ✅ Many-to-many: Professors teaching this course
        public virtual ICollection<ProfessorCourse> ProfessorCourses { get; set; } = new List<ProfessorCourse>();
    }
}
