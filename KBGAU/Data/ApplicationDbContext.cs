using Microsoft.EntityFrameworkCore;
using KBGAU.Models;

namespace KBGAU.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<CourseInfo> CourseInfos { get; set; }
    }
}
