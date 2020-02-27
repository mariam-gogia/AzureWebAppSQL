using System;
using System.ComponentModel.DataAnnotations;

namespace Tasks.DataTransferObjects
{
    /// <summary>
    /// The task updating shape
    /// </summary>
    public class TaskUpdatePayload
    {
        /// <summary>
        /// Gets or sets the task name
        /// </summary>
        [Required(ErrorMessage = "7")]
        [StringLength(100, ErrorMessage = "2")]
        public string TaskName { get; set; }


        /// <summary>
        /// Gets or sets boolean value (true/false) to isCompleted 
        /// </summary>
        /// <value>
        /// <c>true</c> if the task is completed <c>false</c>.
        /// </value>
        [Required(ErrorMessage = "3")]
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Gets or sets due date 
        /// </summary>
        [Required(ErrorMessage = "3")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return $"Name=[{TaskName}], isCompleted=[{IsCompleted}], DueDate=[{DueDate}]";
        }
    }
}
