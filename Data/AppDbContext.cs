using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using kalamon_University.Models.Entities;
using System;
using WebApplication2;

namespace kalamon_University.Data
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<Professor> Professors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }  // Make sure this is present!
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<ProfessorCourse> ProfessorCourses { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Enrollment keys & relationships
            builder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.ProfessorCourseId });

                entity.HasOne(e => e.Student)
                      .WithMany(s => s.Enrollments)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.ProfessorCourse)
                      .WithMany(pc => pc.Enrollments)
                      .HasForeignKey(e => e.ProfessorCourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ProfessorCourse relationship
            builder.Entity<ProfessorCourse>(entity =>
            {
                entity.HasKey(pc => pc.Id);

                entity.HasOne(pc => pc.Professor)
                      .WithMany(p => p.ProfessorCourses)
                      .HasForeignKey(pc => pc.ProfessorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Course)
                      .WithMany(c => c.ProfessorCourses)
                      .HasForeignKey(pc => pc.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // User to Student & Professor profiles one-to-one
            builder.Entity<User>()
                .HasOne(u => u.StudentProfile)
                .WithOne(s => s.User)
                .HasForeignKey<Student>(s => s.UserId);

            builder.Entity<User>()
                .HasOne(u => u.ProfessorProfile)
                .WithOne(p => p.User)
                .HasForeignKey<Professor>(p => p.UserId);

            builder.Entity<Student>(entity =>
            {
                entity.HasIndex(s => s.UserId).IsUnique();
            });

            // Warning enum conversion
            builder.Entity<Warning>()
                .Property(w => w.Type)
                .HasConversion<string>();

            // *** Attendance relations - Use Restrict on all FKs to avoid multiple cascade paths ***
builder.Entity<Attendance>(entity =>
{
    entity.HasOne(a => a.Student)
          .WithMany(s => s.Attendances)
          .HasForeignKey(a => a.StudentId)
          .OnDelete(DeleteBehavior.Restrict);

    entity.HasOne(a => a.ProfessorCourse)
          .WithMany()
          .HasForeignKey(a => a.ProfessorCourseId)
          .OnDelete(DeleteBehavior.Restrict);
});

            // Warning relations
            builder.Entity<Warning>(entity =>
            {
                entity.HasOne(w => w.Student)
                      .WithMany(s => s.Warnings)
                      .HasForeignKey(w => w.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(w => w.Course)
                      .WithMany(c => c.Warnings)
                      .HasForeignKey(w => w.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Notification relations
            builder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.TargetUser)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Roles seeding example
            var adminRoleId = Guid.NewGuid();
            var professorRoleId = Guid.NewGuid();
            var studentRoleId = Guid.NewGuid();

            builder.Entity<IdentityRole<Guid>>().HasData(
                new IdentityRole<Guid> { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<Guid> { Id = professorRoleId, Name = "Professor", NormalizedName = "PROFESSOR" },
                new IdentityRole<Guid> { Id = studentRoleId, Name = "Student", NormalizedName = "STUDENT" }
            );
        }
    }
}
