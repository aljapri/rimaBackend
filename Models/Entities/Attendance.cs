using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kalamon_University.Models.Entities
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;

        public DateTime SessionDate { get; set; } = DateTime.UtcNow;

        public bool IsPresent { get; set; }

        public string? Notes { get; set; }
    }
}
