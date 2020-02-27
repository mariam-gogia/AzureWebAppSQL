using System;
using System.Linq;

namespace Tasks.Data
{
    /// <summary>
    /// Initialize seeds 
    /// </summary>
    public class DbInitializer
    {
        /// <summary>
        /// Initializes the specified context with data
        /// </summary>
        /// <param name="context"></param>
        public static void Initialize(MyDBContext context)
        {
            
            // check if there is any data in Tasks table
            if (context.Tasks.Any())
            {
                return;
            }
            // create date time instances to load it to data
            DateTime d1 = new DateTime(2020, 02, 03);
            DateTime d2 = new DateTime(2020, 01, 01);
            DateTime d3 = new DateTime(2020, 03, 15);
            DateTime d4 = new DateTime(2020, 06, 11);

            // create data
            Models.TaskClass[] tasks = new Models.TaskClass[]
            {
                new Models.TaskClass() {TaskName = "Buy groceries", IsCompleted = false , DueDate = d1},
                new Models.TaskClass() {TaskName = "Workout", IsCompleted = true, DueDate=d2},
                new Models.TaskClass() {TaskName = "Paint fence", IsCompleted = false, DueDate=d3},
                new Models.TaskClass() {TaskName = "Mow Lawn", IsCompleted = false, DueDate=d4},
            };

            // add the data to memory mondel
            foreach (Models.TaskClass task in tasks)
            {
                context.Tasks.Add(task);
            }
            // commit the changes to the database
            context.SaveChanges();
        }
    }
}
