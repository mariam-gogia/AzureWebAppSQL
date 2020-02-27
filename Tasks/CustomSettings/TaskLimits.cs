namespace Tasks.CustomSettings
{
    public class TaskLimits
    {
        /// <summary>
        /// Gets or sets the maximum number of tasks.
        /// </summary>
        /// <value>
        /// The maximum number of tasks.
        /// </value>
        /// <remarks>Setting a default max tasks if not provided in config</remarks>
        public int MaxTasks { get; set; } = 100;
    }
}
