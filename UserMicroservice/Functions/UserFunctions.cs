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
        private readonly IJWTService _jWTService;

        public UserFunctions(IUserRL userRL, IJWTService jWTService)
        {
            this.userRL = userRL;
            this._jWTService = jWTService;
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
        public async Task<IActionResult> GetUsers(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/GetAll")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            
            var response = await this.userRL.GetUsers();

            return new OkObjectResult(response);

        }

        [FunctionName("GetUserById")]
        [OpenApiOperation(operationId: "GetUserById", tags: new[] { "UserService" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "email", In = OpenApiSecurityLocationType.Query)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The id parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> GetUserById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/Get/{id}")] HttpRequest req, string id)
        {

            var response = await this.userRL.GetUserById(id);

            return new OkObjectResult(response);
        }

        [FunctionName("ForgetPassword")]
        [OpenApiOperation(operationId: "ForgetPassword", tags: new[] { "UserService" })]
        [OpenApiSecurity("function_key", SecuritySchemeType.ApiKey, Name = "email", In = OpenApiSecurityLocationType.Query)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ForgetPasswordDetails), Required = true, Description = "Forget Password details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> ForgetPassword(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "user/ForgetPassword")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject<ForgetPasswordDetails>(requestBody);

                var result = this.userRL.ForgetPassword(data);
                if (result != null)
                {
                    return new OkObjectResult(result);
                }
                return new BadRequestResult();
            }
            catch (CosmosException cosmosException)
            {

                log.LogError(" forget password failed with error {0}", cosmosException.ToString());
                return new BadRequestObjectResult($"Failed to proced for forget password Cosmos Status Code {cosmosException.StatusCode}, Sub Status Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
        }

        [FunctionName("ResetPassword")]
        [OpenApiOperation(operationId: "ResetPassword", tags: new[] { "UserService" })]
        [OpenApiSecurity("BearerToken", SecuritySchemeType.ApiKey, Name = "token", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(ResetPasswordDetails), Required = true, Description = "Reset Password details")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> ResetPassword(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "user/ResetPassword")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            try
            {
                var authResponse = _jWTService.ValidateJWT(req);
                if (!authResponse.IsValid)
                {
                    return new UnauthorizedResult();
                }

                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject<ResetPasswordDetails>(requestBody);

                var result = this.userRL.ResetPassword(data);
                if (result != null)
                {
                    return new OkObjectResult(result);
                }
                return new BadRequestResult();
            }
            catch (CosmosException cosmosException)
            {

                log.LogError(" forget password failed with error {0}", cosmosException.ToString());
                return new BadRequestObjectResult($"Failed to proced for forget password Cosmos Status Code {cosmosException.StatusCode}, Sub Status Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
        }
    }
}

