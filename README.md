# AzureWebAppSQL
Azure web app employing SQL database

***Project***

REST interface that employs .NET Core API that uses Entity Framework Core and Azure SQL for creation and management of list of tasks.

Azure SQL PaaS is used to host the database. The application is deployed to Microsoft Azure App Service and can be found at:
https://tasksdbhw2.azurewebsites.net

**Framework:**
Visual Studio 2019, C#, ASP.NET Core, LINQ, Azure SQL, .NET Core API 3.1, Entity Framework Core, MS Tests.

**Overview:**

The application supports up to 100 tasks. Communication is done entirely using JSON objects.
User can create, delete, retrieve tasks by id, and retrieve all tasks.

Each of this action is reviewed in detail below.

Project uses customized error responses, details about error responses are provided at the end of this document.

**1. API operation - Create Task**

Implemented as HTTP POST action.

Request Body:
{ 
       “taskName” : “the task name”,
       “isCompleted”: true or false,
       “dueDate”: the date the task is due in ISO 8601 format”
}

Responses:
1. 201 (Created) - if the task was created successfully 

Response body example for 201(Created):
<pre>
{ 
       “id”: 1
       “taskName” : “Buy Groceries”,
       “isCompleted” : false,
       “dueDate” :  “2020-04-23”
}
</pre>

2. 400 (Bad Request) – returned if the task has one or more invalid parameters
Response body example for 400(Bad Request):

<pre>
{ 
       “errorNumber” : 2
       “parameterName” : “taskName”,
       “parameterValue” :“value provided that cause the error”,
       “errorDescription” : “The parameter is too large”
}
</pre>

3. 403 (Forbidden) – user attempted to create 101th task.
Response body example for 403(Forbidden):
<pre>
{ 
       “errorNumber” : 4
       “parameterName” : null,
       “parameterValue” : null,
       “errorDescription” :  “The maximum number of entities have been created. No further entities can be created at this time”
}
</pre>
4. 409 (Conflict) – user attempted to create the same task.
Response body example for 409(Conflict):
<pre>
{ 
       “errorNumber” : 1
       “parameterName” : “taskName”,
       “parameterValue” : “value provided that cause the error”,
       “errorDescription:”  “The entity already exists”
}
</pre>
**2. API Operation – Update Task**
If user provides the task id, she/he can alter the body of that task
Implemented as HTTP PATCH action

URI Parameter: id
<pre>
Request body:
{ 
       “taskName” : “the task name”,
       “isCompleted”: true or false,
       “dueDate”: the date the task is due in ISO 8601 format”
}
</pre>
Responses:
(bodies with the same customization as in Create Task operation) 

<pre>
1. 204(No Content) – return in case of success
2. 400 (Bad Request) 
3. 409 (Conflict) – return if user tries to put already existing task using different id.
</pre>

**3. API Operation – Delete**

Deletes task with specified id 
Implemented as HTTP DELETE action

URI Parameter: id

Responses
(bodies with the same customization as in Create Task operation) 
<pre>
1. 204(No Content) – success
2. 404(Not Found) – a task with given id cannot be found
</pre>
**4. API Operation – Retrieve by Id**

Gets task with specified id
Implemented as HTTP GET action

URI parameter: id

1. 200 (OK) response body:
<pre>
{
       “id” :numeric id of the task,
       “taskName”: “the task name”,
       “dueDate”: “the date the task is due in ISO 8601 format”
       “isCompleted”: true or false
}
</pre>
2. 404 (Not Found)

**5. API Operation – Retrieve all**

Retrieves all tasks
Implemented as HTTP GET action

URI parameters (Optional) : orderByDate, taskStatus
Responses:
<pre>
1. 200 (OK) response body:
{ 
      "tasks": [
        {
	"id":numeric id of task 1,
	"taskName":"the name of task 1",
	"dueDate":"the date the task is due in ISO 8601 format of task 1",
	"isCompleted":false
        },
        {
	"id":numeric id of task 2,
	"taskName":"the name of task 2",
	"dueDate":"the date the task is due in ISO 8601 format of task 2",
	"isCompleted":false
         },
     ]
}
</pre>
2. 400 (Bad Request) -  return if values in optional parameter fields are incorrect
**Errors**

All http 400 and 404 errors return response body using the template below.
Error responses are manually created and numbers are arbitrarily assigned to them.
<pre>
Error Response JSON:
{
	"errorNumber": error number,
	"parameterName": " name of parameter that caused the error" ,
	"parameterValue":"value of parameter that caused the error",
	"errorDescription":"Description of the error"
}
</pre>


• Project includes client SDK of the REST interface which is utilized by MS Tests. MS Tests test the functionality of the REST interface
