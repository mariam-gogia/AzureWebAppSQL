using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tasks.Data;
using Tasks.DataTransferObjects;
using Tasks.CustomSettings;
using Tasks.Common;
using Tasks.Models;
using System.IO;
using System.Text.Json;



namespace Tasks.Controllers
{
    [Route("api/[controller]")]
    [Produces ("application/json")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        /// <summary>
        /// get task by identifier route
        /// </summary>
        private const string GetTaskByIdRoute = "GetTaskByIdRoute";

        /// <summary>
        /// The database context
        /// </summary>
        private readonly MyDBContext _context;

        /// <summary>
        /// logger instance
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// The cofiguration instance
        /// </summary>
        private readonly IConfiguration _configuration;

        /// <summary>
        /// tasks limit setting
        /// </summary>
        private readonly TaskLimits _taskLimits;

        /// <summary>
        /// Initializes a new instance of the <see cref="TasksController"/> class.
        /// </summary>
        public TasksController(ILogger<TasksController> logger,
                                   MyDBContext context,
                                   IConfiguration configuration,
                                   IOptions<TaskLimits> taskLimits)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
            _taskLimits = taskLimits.Value;
        }

        /// <summary>
        /// Retrieves a task from the database by id
        /// </summary>
        /// <param name="id"> The id of the task that needs to be retrieved </param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(typeof(TaskResultPayload), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Route("{id}", Name = GetTaskByIdRoute)]
        public IActionResult GetTaskById(long id)
        {
            try
            {
                TaskClass task = (from c in _context.Tasks where c.Id == id orderby c.TaskName descending select c).SingleOrDefault();
                // check if the task by given id exists
                if (task == null)
                {
                    _logger.LogInformation(LoggingEvents.GetItem, $"TasksController Task(id=[{id}]) was not found.");
                    ErrorResponse errorResponse = new ErrorResponse
                    {
                        errorNumber = 5,
                        errorDescription = "The entity could not be found",
                        parameterName = "id",
                        parameterValue = id.ToString()
                    };
                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }
                return new ObjectResult(new TaskResultPayload(task));
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TasksController Get Task(id=[{id}]) caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }     
        }

        /// <summary>
        /// helper method to determine whether or not tasks exceeded the limit
        /// </summary>
        /// <returns></returns>
        private bool CanAddMoreTasks()
        {
            long totalTasks = (from c in _context.Tasks select c).Count();
            return _taskLimits.MaxTasks > totalTasks;
        }

        /// <summary>
        /// Adds valid tasks to the database 
        /// </summary>
        /// <param name="taskCreatePayload"> taskName, isCompleted and DueDate are required fields</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(TaskResultPayload), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] TaskCreatePayload taskCreatePayload)
        {
            // create a new TaskClass instance 
            TaskClass taskEntity = new TaskClass();
            try
            {
                if (ModelState.IsValid)
                {
                    // check if there is enough space
                    if (CanAddMoreTasks() == false)
                    {
                        ErrorResponse errorResponse = new ErrorResponse
                        {
                            errorNumber = 4,
                            errorDescription = "The maximum number of entities have been created. No further entities can be created at this time."
                        };
                        return StatusCode((int)HttpStatusCode.Forbidden, errorResponse);
                    }
                    // check if the task already exists
                    bool taskExists = (from c in _context.Tasks where c.TaskName == taskCreatePayload.TaskName select c).Any();
                    if (taskExists)
                    {
                        ErrorResponse errorResponse = new ErrorResponse
                        {
                            errorNumber = 1,
                            errorDescription = "The entity already exists",
                            parameterName = "taskName",
                            parameterValue = taskCreatePayload.TaskName
                        };
                        return StatusCode((int)HttpStatusCode.Conflict, errorResponse);
                    }
                    // add taskEntity to the database
                    taskEntity.TaskName = taskCreatePayload.TaskName;
                    taskEntity.IsCompleted = taskCreatePayload.IsCompleted;
                    taskEntity.DueDate = taskCreatePayload.DueDate;
                    _context.Tasks.Add(taskEntity);
                    _context.SaveChanges();
                }
                else
                {
                    // check badRequest erros in properties of taskCreatePayload and return it as bad request
                    ErrorResponse errorResponse = new ErrorResponse();
                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();
                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        foreach (string key in ModelState.Keys)
                        {
                            if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                                {
                                    string cleansedKey = key.CleanseModelStateKey();
                                    System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemtedValue:{jsonDocument.RootElement.GetProperty(cleansedKey.ToCamelCase())}, errorMessage:{error.ErrorMessage}");
                                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage(error.ErrorMessage);
                                    errorResponse.parameterName = cleansedKey;
                                    errorResponse.parameterValue = jsonDocument.RootElement.GetProperty(cleansedKey.ToCamelCase()).ToString();  
                                }
                            }
                        }
                    }
                    return BadRequest(errorResponse);
                }

            } catch(Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TasksController Post TaskEntity([{taskEntity}]) " +
                    $"TaskCreatePayload([{taskCreatePayload}] caused an internal error.");
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
           return CreatedAtRoute(GetTaskByIdRoute, new { id = taskEntity.Id }, new TaskResultPayload(taskEntity));
        }
        /// <summary>
        /// Updates the parameters of an already existing task
        /// </summary>
        /// <param name="id"> id of the task that needs to be updated </param>
        /// <param name="taskUpdatePayload"></param>
        /// <returns> success 204 without any content if successful </returns>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        [HttpPatch("{id}")]
        public async Task <IActionResult> UpdateTask (long id, [FromBody] TaskUpdatePayload taskUpdatePayload)
        {
            try
            {
                if (ModelState.IsValid)
                {   
                    // check if the task with given id exists in the database
                    TaskClass taskEntity = (from c in _context.Tasks where c.Id == id select c).SingleOrDefault();
                    if (taskEntity == null)
                    {
                        _logger.LogInformation(LoggingEvents.UpdateItem, $"TasksController Task(id=[{id}]) was not found.", id);
                        ErrorResponse errorResponse = new ErrorResponse
                        {
                            errorNumber = 5,
                            errorDescription = "The entity could not be found",
                            parameterName = "id",
                            parameterValue = id.ToString()
                        };
                        return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                    }
                    // check if there is a conflict (same task names, different ids)
                    TaskClass taskNamecheck = (from c in _context.Tasks where c.TaskName == taskUpdatePayload.TaskName select c).SingleOrDefault();
                    if(taskNamecheck!= null && taskNamecheck.Id != id)
                    {
                        ErrorResponse errorResponse = new ErrorResponse
                        {
                            errorNumber = 1,
                            errorDescription = "The entity already exists",
                            parameterName = "taskName",
                            parameterValue = taskUpdatePayload.TaskName
                        };
                        return StatusCode((int)HttpStatusCode.Conflict, errorResponse);
                    }
                    // Update the entity specified by the caller.
                    taskEntity.TaskName = taskUpdatePayload.TaskName;
                    taskEntity.IsCompleted = taskUpdatePayload.IsCompleted;
                    taskEntity.DueDate = taskUpdatePayload.DueDate;
                    _context.SaveChanges();
                }
                else
                {
                    // check if there is any errors in provided requesy body parameters
                    ErrorResponse errorResponse = new ErrorResponse();
                    using StreamReader sr = new StreamReader(Request.Body);
                    Request.Body.Seek(0, SeekOrigin.Begin);
                    string inputJsonString = await sr.ReadToEndAsync();
                    using (JsonDocument jsonDocument = JsonDocument.Parse(inputJsonString))
                    {
                        foreach (string key in ModelState.Keys)
                        {
                            if (ModelState[key].ValidationState == Microsoft.AspNetCore.Mvc.ModelBinding.ModelValidationState.Invalid)
                            {
                                foreach (Microsoft.AspNetCore.Mvc.ModelBinding.ModelError error in ModelState[key].Errors)
                                {
                                    string cleansedKey = key.CleanseModelStateKey();
                                    System.Diagnostics.Trace.WriteLine($"MODEL ERROR: key:{cleansedKey} attemtedValue:{jsonDocument.RootElement.GetProperty(cleansedKey.ToCamelCase())}, errorMessage:{error.ErrorMessage}");
                                    (errorResponse.errorDescription, errorResponse.errorNumber) = ErrorResponse.GetErrorMessage(error.ErrorMessage);
                                    errorResponse.parameterName = cleansedKey;
                                    errorResponse.parameterValue = jsonDocument.RootElement.GetProperty(cleansedKey.ToCamelCase()).ToString();
                                }
                            }
                        }
                    }
                    return BadRequest(errorResponse);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TaskController Patch Task(id=[{id}]) caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            return NoContent();
        }

        /// <summary>
        /// Deletes tasks from the database
        /// </summary>
        /// <param name="id"> id of the task that needs to be deleted</param>
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [Route("{id}")]
        [HttpDelete]
        public IActionResult DeleteTaskById(long id)
        {
            try
            {
                TaskClass dbTask = (from c in _context.Tasks where c.Id == id select c).SingleOrDefault();
                // check if the task exists
                if (dbTask == null)
                {
                    _logger.LogInformation(LoggingEvents.UpdateItem, $"TasksController Task(id=[{id}]) was not found.", id);
                    ErrorResponse errorResponse = new ErrorResponse
                    {
                        errorNumber = 5,
                        errorDescription = "The entity could not be found",
                        parameterName = "id",
                        parameterValue = id.ToString()
                    };
                    return StatusCode((int)HttpStatusCode.NotFound, errorResponse);
                }
                // if exists, remove 
                _context.Tasks.Remove(dbTask);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(LoggingEvents.InternalError, ex, $"TasksController Delete Customer(id=[{id}]) caused an internal error.", id);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
            return NoContent();
        }

        /// <summary>
        /// Gets the full list of tasks (options to sort/filter)
        /// </summary>
        /// <param name="orderByDate"> Can be Asc or Desc</param>
        /// <param name="taskStatus"> Can be Completed, NotCompleted or All</param>
        /// <returns> the list of tasks</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IDictionary<string, List<TaskResultPayload>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public IActionResult GetAllTasks(string orderByDate = "Asc", string taskStatus = "All")
        {

            // error checking for correct strings
            ErrorResponse errorResponse = new ErrorResponse();

            // check if orderBy string is too large and if it contains right values
            if (orderByDate.Length > 100)
            {
                errorResponse.errorNumber = 2;
                errorResponse.errorDescription = "The parameter value is too large";
                errorResponse.parameterName = "orderByDate";
                errorResponse.parameterValue = orderByDate;
                return BadRequest(errorResponse);
            }
            if (orderByDate.ToLower() != "asc" && orderByDate.ToLower() != "desc")
            {
                errorResponse.errorNumber = 7;
                errorResponse.errorDescription = "The parameter value is not valid";
                errorResponse.parameterName = "orderByDate";
                errorResponse.parameterValue = orderByDate;
                return BadRequest(errorResponse);
            }
            // check if taskStatus is too large and if it contains valid values
            if (taskStatus.Length > 100)
            {
                errorResponse.errorNumber = 2;
                errorResponse.errorDescription = "The parameter value is too large";
                errorResponse.parameterName = "taskStatus";
                errorResponse.parameterValue = taskStatus;
                return BadRequest(errorResponse);
            }
            if (taskStatus.ToLower() != "completed" && taskStatus.ToLower() != "notcompleted" && taskStatus.ToLower() != "all")
            {
                errorResponse.errorNumber = 7;
                errorResponse.errorDescription = "The parameter value is not valid";
                errorResponse.parameterName = "taskStatus";
                errorResponse.parameterValue = taskStatus;
                return BadRequest(errorResponse);
            }

            List<TaskResultPayload> tasks = new List<TaskResultPayload>(); //list that will be returned at the end

            IEnumerable<TaskClass> taskByDateAsc = (from c in _context.Tasks orderby c.DueDate select c); //stores tasks sorted ascending
            IEnumerable<TaskClass> taskByDateDesc = (from c in _context.Tasks orderby c.DueDate descending select c); //stores tasks sorted descending
                                                                                                                      // lists that will store converted TaskClass objcets to TaskResultPayload objects in specified orders
            List<TaskResultPayload> tasksAsc = new List<TaskResultPayload>();
            List<TaskResultPayload> tasksDesc = new List<TaskResultPayload>();
            // converting TaskClass objects from the database to TaskResultPayload Objects
            foreach (TaskClass t in taskByDateDesc)
            {
                TaskResultPayload copy = new TaskResultPayload(t);
                tasksDesc.Add(copy);
            }
            foreach (TaskClass t in taskByDateAsc)
            {
                TaskResultPayload copy = new TaskResultPayload(t);
                tasksAsc.Add(copy);
            }

            if (orderByDate.ToLower() == "desc")
            {
                if (taskStatus.ToLower() == "completed")
                {
                    foreach (TaskResultPayload t in tasksDesc)
                    {
                        if (t.IsCompleted == true)
                        {
                            tasks.Add(t);
                        }
                    }
                }
                else if (taskStatus.ToLower() == "notcompleted")
                {
                    foreach (TaskResultPayload t in tasksDesc)
                    {
                        if (t.IsCompleted == false)
                        {
                            tasks.Add(t);
                        }
                    }
                }
                else
                {
                    foreach (TaskResultPayload t in tasksDesc)
                    {
                        tasks.Add(t);
                    }
                }
                return new ObjectResult(tasks);
            }

            else
            {
                if (taskStatus.ToLower() == "completed")
                {
                    foreach (TaskResultPayload t in tasksAsc)
                    {
                        if (t.IsCompleted == true)
                        {
                            tasks.Add(t);
                        }
                    }
                }
                else if (taskStatus.ToLower() == "notcompleted")
                {
                    foreach (TaskResultPayload t in tasksAsc)
                    {
                        if (t.IsCompleted == false)
                        {
                            tasks.Add(t);
                        }
                    }
                }
                else
                {
                    foreach (TaskResultPayload t in tasksAsc)
                    {
                        tasks.Add(t);
                    }
                }
            }
            //return dictionary with key "tasks:" and list of tasks
            IDictionary<string, List<TaskResultPayload>> map = new Dictionary<string, List<TaskResultPayload>>();
            map.Add("tasks:", tasks);
            return new ObjectResult(map);
        }
    }
}
