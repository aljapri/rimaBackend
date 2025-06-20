using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace kalamon_University.Models.Entities
{
   public class ProfessorCourse
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid ProfessorId { get; set; }
        public virtual Professor Professor { get; set; } = null!;

        public int CourseId { get; set; }
        public virtual Course Course { get; set; } = null!;

        public bool Practical { get; set; }
        public bool Theoretical { get; set; }
        public int PracticalN { get; set; }
        public int TheoreticalN { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
