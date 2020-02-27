using Microsoft.VisualStudio.TestTools.UnitTesting;
using RestClientSDKlibrary;
using RestClientSDKlibrary.Models;
using System;
using System.Threading.Tasks;
using Microsoft.Rest;
using Microsoft.AspNetCore.Http;

namespace FunctionTests
{
    [TestClass]
    public class FunctionalTest
    {
        [TestClass]
        public class Functional
        {
            ServiceClientCredentials _serviceClientCredentials;
            RestClientSDKlibraryClient _client;

            [TestInitialize]
            public void Initialize()
            {
                _serviceClientCredentials = new TokenCredentials("FakeTokenValue");

                _client = new RestClientSDKlibraryClient(
                    new Uri("https://tasksdbhw2.azurewebsites.net/"), _serviceClientCredentials);
            }
            [TestMethod]
            public async Task ConfirmConflictOnCreate()
            {
                // arrange 
                TaskCreatePayload testInput = new TaskCreatePayload
                {
                    TaskName = "Buy groceries",
                    IsCompleted = true,
                    DueDate = DateTime.Today
                };

                // entity already exists
                int errorNumber = 1;
                string errorParameterName = "taskName";
                string errorParameterValue = "Buy groceries";

                // act
                object outputStatus = _client.CreateTask(testInput);
                ErrorResponse errorResponse = outputStatus as ErrorResponse;

                // assert error response
                Assert.AreEqual(errorNumber, errorResponse.ErrorNumber);
                Assert.AreEqual(errorParameterName, errorResponse.ParameterName);
                Assert.AreEqual(errorParameterValue, errorResponse.ParameterValue);

                // Assert error code
                HttpOperationResponse<object> outputPayload = await _client.CreateTaskWithHttpMessagesAsync(testInput);
                Assert.AreEqual(StatusCodes.Status409Conflict, (int)outputPayload.Response.StatusCode);
            }
            [TestMethod]
            public async Task CreateWithEmptyString()
            {
                // arrange 
                TaskCreatePayload testInput = new TaskCreatePayload
                {
                    TaskName = "",
                    IsCompleted = true,
                    DueDate = DateTime.Today
                };
                // invalid input error
                int errorNumber = 7;
                string errorParameterName = "TaskName";
                string errorParameterValue = "";

                // act
                object outputStatus = _client.CreateTask(testInput);
                ErrorResponse errorResponse = outputStatus as ErrorResponse;

                // assert error response
                Assert.AreEqual(errorNumber, errorResponse.ErrorNumber);
                Assert.AreEqual(errorParameterName, errorResponse.ParameterName);
                Assert.AreEqual(errorParameterValue, errorResponse.ParameterValue);

                // assert status code
                HttpOperationResponse<object> outputPayload = await _client.CreateTaskWithHttpMessagesAsync(testInput);
                System.Diagnostics.Debug.WriteLine(outputPayload.Request.Content.ToString());
                Assert.AreEqual(StatusCodes.Status400BadRequest, (int)outputPayload.Response.StatusCode);
            }
            [TestMethod]
            public async Task DeleteErrorAndStatusCodeCheck()
            {
                // arrange error - value does not exit
                int id = 600;
                int errorNumber = 5;
                string errorParameterName = "id";
                string errorParameterValue = id.ToString();

                // act
                object outputStatusTest2 = _client.DeleteTaskById(id);
                ErrorResponse errorResponse = outputStatusTest2 as ErrorResponse;

                // assert error response
                Assert.AreEqual(errorNumber, errorResponse.ErrorNumber);
                Assert.AreEqual(errorParameterName, errorResponse.ParameterName);
                Assert.AreEqual(errorParameterValue, errorResponse.ParameterValue);

                // assert status code
                HttpOperationResponse<ErrorResponse> outputPayload = await _client.DeleteTaskByIdWithHttpMessagesAsync(id);
                Assert.AreEqual(StatusCodes.Status404NotFound, (int)outputPayload.Response.StatusCode);
            }
            [TestMethod]
            public async Task DeleteTest()
            {
                // this test creates and deletes the task
                // arrange 
                TaskCreatePayload testInput = new TaskCreatePayload
                {
                    TaskName = "Testing String",
                    IsCompleted = true,
                    DueDate = DateTime.Today
                };

                // act
                object outputStatus = _client.CreateTask(testInput);
                TaskResultPayload taskResult = outputStatus as TaskResultPayload;

                Console.WriteLine(taskResult.TaskName);
                long? id = taskResult.Id;

                HttpOperationResponse <ErrorResponse> outputStatusTest2 = await _client.DeleteTaskByIdWithHttpMessagesAsync((long)id);

                // Assert error code
                Assert.AreEqual(StatusCodes.Status204NoContent, (int)outputStatusTest2.Response.StatusCode);
            }
            
            [TestMethod]
            public void GetTaskByIdNotFoundTest()
            {
                // arrange error response we must get (error 5 - not found)
                int id = 150;
                int errorNumber = 5;
                string parameterName = "id";
                string parameterValue = id.ToString();

                // act
                object o = _client.GetTaskById(id);
                ErrorResponse errorTest = o as ErrorResponse;

                // assert
                Assert.AreEqual(errorNumber, errorTest.ErrorNumber);
                Assert.AreEqual(parameterName, errorTest.ParameterName);
                Assert.AreEqual(parameterValue, errorTest.ParameterValue);
            }
            [TestMethod]
            public async Task GetTaskByIdStatusAndNameCheck()
            {
                // arrange 
                int id = 1;
                string name = "Buy groceries";

                // act
                object o = _client.GetTaskById(id);
                TaskResultPayload resultPayload = o as TaskResultPayload;
                HttpOperationResponse<object> outputStatusTest = await _client.GetTaskByIdWithHttpMessagesAsync(id);

                // test if the name matches the taskName & returns the right status code
                Assert.AreEqual(name, resultPayload.TaskName);
                Assert.AreEqual(StatusCodes.Status200OK, (int)outputStatusTest.Response.StatusCode);
            }
            [TestMethod]
            public async Task UpdateConflict()
            {
                //arrange 
                TaskUpdatePayload testInput = new TaskUpdatePayload
                {
                    TaskName = "Buy groceries",
                    IsCompleted = true,
                    DueDate = DateTime.Today
                };

                // trying to pass a task with the same name but different id
                int id = 4;

                // already exists 
                int errorNumber = 1;
                string errorParameterName = "taskName";
                string errorParameterValue = "Buy groceries";

                // act
                object outputStatus = _client.UpdateTask(id, testInput);
                ErrorResponse errorResponse = outputStatus as ErrorResponse;

                // assert error response
                Assert.AreEqual(errorNumber, errorResponse.ErrorNumber);
                Assert.AreEqual(errorParameterName, errorResponse.ParameterName);
                Assert.AreEqual(errorParameterValue, errorResponse.ParameterValue);

                // Assert error code
                HttpOperationResponse<ErrorResponse> outputPayload = await _client.UpdateTaskWithHttpMessagesAsync(id, testInput);
                Assert.AreEqual(StatusCodes.Status409Conflict, (int)outputPayload.Response.StatusCode);
            }
            [TestMethod]
            public async Task RetrieveAllwrongParam()
            {
                // arrange
                string wrongParam = "randomOrder";
                
                // invalid value
                int errorNumber = 7;
                string errorParameterName = "orderByDate";
                string errorParameterValue = wrongParam;

                // act
                object outputStatus = _client.GetAllTasks(wrongParam);
                ErrorResponse errorResponse = outputStatus as ErrorResponse;

                // assert error response
                Assert.AreEqual(errorNumber, errorResponse.ErrorNumber);
                Assert.AreEqual(errorParameterName, errorResponse.ParameterName);
                Assert.AreEqual(errorParameterValue, errorResponse.ParameterValue);

                // Assert error code
                HttpOperationResponse<object> outputPayload = await _client.GetAllTasksWithHttpMessagesAsync(wrongParam);
                Assert.AreEqual(StatusCodes.Status400BadRequest, (int)outputPayload.Response.StatusCode);
            }
            [TestMethod]
            public async Task RetrieveAllParamTooLarge()
            {
                // arrange
                string wrongParam = "53TfBj0LwlkiN3wgT3eUN8obrMe2qFQUoFihAqnOiEU6uXP0yj66rwZnMOZnsjSirs3a1Ttp5VQawSuQnhi9ZLADyGOA5wNETe980i";

                // string too large
                int errorNumber = 2;
                string errorParameterName = "orderByDate";
                string errorParameterValue = wrongParam;

                // act
                object outputStatus = _client.GetAllTasks(wrongParam);
                ErrorResponse errorResponse = outputStatus as ErrorResponse;

                // assert error response
                Assert.AreEqual(errorNumber, errorResponse.ErrorNumber);
                Assert.AreEqual(errorParameterName, errorResponse.ParameterName);
                Assert.AreEqual(errorParameterValue, errorResponse.ParameterValue);

                // Assert error code
                HttpOperationResponse<object> outputPayload = await _client.GetAllTasksWithHttpMessagesAsync(wrongParam);
                Assert.AreEqual(StatusCodes.Status400BadRequest, (int)outputPayload.Response.StatusCode);
            }
        }
    }
}

        
        
    


