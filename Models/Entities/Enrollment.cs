using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace kalamon_University.Models.Entities
{
    public class Enrollment
    {
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow;

        public bool IsBannedFromExam { get; set; } = false;
    }
}
