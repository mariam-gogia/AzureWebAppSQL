using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tasks.Models
{
    /// <summary>
    /// The Task entity
    /// </summary>
    public class TaskClass
    {
        /// <summary>
        /// Gets or sets the task identifier.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long? Id { get; set; }
        
        /// <summary>
        /// Gets or sets the task description
        /// </summary>
        [Required]
        [StringLength(100)]
        public string TaskName { get; set; }

        /// <summary>
        /// Gets or sets boolean value (true/false) to isCompleted 
        /// </summary>
        /// <value>
        /// <c>true</c> if the task is completed <c>false</c>.
        /// </value>
        [Required]
        public bool IsCompleted { get; set; } = false;
       
        /// <summary>
        /// Gets or sets the due date
        /// </summary>
        [Required]
        public DateTime DueDate { get; set; }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return $"Id=[{Id}] Name=[{TaskName}], isCompleted=[{IsCompleted}], DueDate=[{DueDate}]";
        }
    }
}
