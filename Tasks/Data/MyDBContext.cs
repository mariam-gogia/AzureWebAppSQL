using Microsoft.EntityFrameworkCore;


namespace Tasks.Data
{
    /// <summary>
    ///  Coordinates Entity Framework functionality for a given data model is the database context class
    /// </summary>
    public class MyDBContext : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MyDBContext"/> class.
        /// </summary>
        /// <param name="options"></param>
        public MyDBContext(DbContextOptions<MyDBContext> options) : base(options)
        {
            // creating database tables
            Database.EnsureCreated();
        }

        /// <summary>
        /// Creating the table Tasks and its accessor and mutator
        /// </summary>
        public DbSet<Models.TaskClass> Tasks { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // adds task to tne entity model linking it to the tasks table
            modelBuilder.Entity<Models.TaskClass>().ToTable("Tasks");
        }
    }
}
