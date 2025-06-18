using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace kalamon_University.Models.Entities
{
    public class ProfessorCourse
    {
        [ForeignKey("Professor")]
        public Guid ProfessorId { get; set; }
        public virtual Professor Professor { get; set; } = null!;

        [ForeignKey("Course")]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;
    }
}
