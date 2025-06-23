using Microsoft.EntityFrameworkCore;
using SurveyApp.Database.Entities;

namespace SurveyApp.Database
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }
		public DbSet<HelloEntity> TestEntity { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
