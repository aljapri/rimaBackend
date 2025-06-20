using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kalamon_University.Models.Entities
{
    public class Enrollment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        public Guid ProfessorCourseId { get; set; }
        public virtual ProfessorCourse ProfessorCourse { get; set; } = null!;

        public DateTime EnrollmentDate { get; set; }
        public bool IsBannedFromExam { get; set; } = false;
    }
}
