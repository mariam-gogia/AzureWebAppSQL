using System;
using System.ComponentModel.DataAnnotations;

namespace Tasks.DataTransferObjects
{
    /// <summary>
    /// Task creation shape
    /// </summary>
    public class TaskCreatePayload
    {
        /// <summary>
        /// Gets or sets the task description
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
        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Get or set the due date
        /// </summary>
        [Required(ErrorMessage = "3")]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return $"Name=[{TaskName}], isCompleted=[{IsCompleted}], DueDate=[{DueDate}]";
        }
    }
}



