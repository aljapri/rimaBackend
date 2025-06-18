using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using kalamon_University.Models.Entities;
using System;

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
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Warning> Warnings { get; set; }
        public DbSet<ProfessorCourse> ProfessorCourses { get; set; } // ✅ added

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ---- Enrollment (Student <-> Course) ----
            builder.Entity<Enrollment>(entity =>
            {
                entity.HasKey(e => new { e.StudentId, e.CourseId });

                entity.HasOne(e => e.Student)
                      .WithMany(s => s.Enrollments)
                      .HasForeignKey(e => e.StudentId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Enrollments)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ---- ProfessorCourse (Many-to-Many: Professor <-> Course) ----
            builder.Entity<ProfessorCourse>(entity =>
            {
                entity.HasKey(pc => new { pc.ProfessorId, pc.CourseId });

                entity.HasOne(pc => pc.Professor)
                      .WithMany(p => p.ProfessorCourses)
                      .HasForeignKey(pc => pc.ProfessorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pc => pc.Course)
                      .WithMany(c => c.ProfessorCourses)
                      .HasForeignKey(pc => pc.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- One-to-One: User <-> Student ----
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

            // ---- Enum Conversion ----
            builder.Entity<Warning>()
                .Property(w => w.Type)
                .HasConversion<string>();

            // ---- Attendance ----
            builder.Entity<Attendance>(entity =>
            {
                entity.HasOne(a => a.Student)
                      .WithMany(s => s.Attendances)
                      .HasForeignKey(a => a.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Course)
                      .WithMany(c => c.Attendances)
                      .HasForeignKey(a => a.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Warning ----
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

            // ---- Notification ----
            builder.Entity<Notification>(entity =>
            {
                entity.HasOne(n => n.TargetUser)
                      .WithMany(u => u.Notifications)
                      .HasForeignKey(n => n.UserId)
                      .IsRequired()
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ---- Role Seeding ----
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
