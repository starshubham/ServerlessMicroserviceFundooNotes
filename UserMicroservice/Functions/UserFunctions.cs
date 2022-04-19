using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CommonLayer.RequestModels;
using CommonLayer.ResponseModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RepositoryLayer.Interfaces;

namespace UserMicroservice
{
    public class UserFunctions
    {
        private readonly IUserRL userRL;
        private readonly CosmosClient _cosmosClient;

        public UserFunctions(IUserRL userRL, CosmosClient cosmosClient)
        {
            this.userRL = userRL;
            this._cosmosClient = cosmosClient;
        }

        [FunctionName("UserRegistration")]
        [OpenApiOperation(operationId: "UserRegistration", tags: new[] { "UserService" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "email", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UserDetails), Required = true, Description = "New user details.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> UserRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/registration")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject<UserDetails>(requestBody);

                var result = await this.userRL.CreateUser(data);
                return new OkObjectResult(result);

            }
            catch (CosmosException cosmosException)
            {
                log.LogError("Creating item failed with error {0}", cosmosException.ToString());
                return new BadRequestObjectResult($"Failed to create item. Cosmos Status Code {cosmosException.StatusCode}, Sub Status Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
        }

        [FunctionName("UserLogin")]
        [OpenApiOperation(operationId: "UserLogin", tags: new[] { "UserService" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "email", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(LoginCredentials), Required = true, Description = "New user details.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(LoginResponse), Description = "The OK response")]
        public async Task<IActionResult> UserLogin(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/login")] HttpRequest req)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<LoginCredentials>(requestBody);
            var response = this.userRL.UserLogin(data);

            return new OkObjectResult(response);
        }

        [FunctionName("GetAllUsers")]
        [OpenApiOperation(operationId: "GetAllUsers", tags: new[] { "UserService" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "email", In = OpenApiSecurityLocationType.Query)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> GetAllUsers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAll")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            QueryDefinition query = new QueryDefinition("select * from UserDetails");

            var container = this._cosmosClient.GetContainer("UserDB", "UserDetails");

            List<UserDetails> userLists = new List<UserDetails>();
            using (FeedIterator<UserDetails> resultSet = container.GetItemQueryIterator<UserDetails>(query))
            {
                while (resultSet.HasMoreResults)
                {
                    FeedResponse<UserDetails> response = await resultSet.ReadNextAsync();
                    UserDetails user = response.First();
                    log.LogInformation($"Id: {user.UserId};");
                    if (response.Diagnostics != null)
                    {
                        Console.WriteLine($" Diagnostics {response.Diagnostics.ToString()}");
                    }

                    userLists.AddRange(response);

                }
            }

            return new OkObjectResult(userLists);

        }

    }
}

