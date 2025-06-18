using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kalamon_University.Models.Entities
{
    public class Professor
    {
        [Required]
        [StringLength(100)]
        public string Specialization { get; set; } = string.Empty;

        // علاقة 1:1 مع User
        [Key, ForeignKey("User")]
        public Guid UserId { get; set; }
        public virtual User User { get; set; } = null!;

        // علاقة Many-to-Many مع Course عبر الربط ProfessorCourse
        public virtual ICollection<ProfessorCourse> ProfessorCourses { get; set; } = new List<ProfessorCourse>();
    }
}
