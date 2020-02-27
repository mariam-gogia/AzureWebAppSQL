using Tasks.Models;

namespace Tasks.DataTransferObjects
{
    /// <summary>
    /// Defines the public facing task attributes
    /// </summary>
    public class TaskResultPayload
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TaskResultPayload"/> class using a TaskClass as input
        /// </summary>
        /// <param name="task"> task </param>
        public TaskResultPayload(TaskClass task)
        {
            Id = task.Id ?? -1;
            TaskName = task.TaskName;
            DueDate = task.DueDate.ToString();
            IsCompleted = task.IsCompleted;
        }
        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// Gets or sets the task name
        /// </summary>
        public string TaskName { get; set; }
        /// <summary>
        /// Gets or sets Due date
        /// </summary>
        public string DueDate { get; set; }
        /// <summary>
        /// Gets or sets boolean value (true/false) to isCompleted 
        /// </summary>
        public bool IsCompleted { get; set; }
    }
}
