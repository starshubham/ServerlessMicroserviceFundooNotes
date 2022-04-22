using System.IO;
using System.Net;
using System.Threading.Tasks;
using CommonLayer.RequestModels;
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

namespace NoteMicroservice
{
    public class NoteFunctions
    {
        private readonly INoteRL noteRL;
        private readonly IJWTService _jWTService;

        public NoteFunctions(INoteRL noteRL, IJWTService jWTService)
        {
            this.noteRL = noteRL;
            this._jWTService = jWTService;
        }

        [FunctionName("CreateNote")]
        [OpenApiOperation(operationId: "CreateNote", tags: new[] { "NoteService" })]
        [OpenApiSecurity("JWT Bearer Token", SecuritySchemeType.ApiKey, Name = "token", In = OpenApiSecurityLocationType.Header)]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(NoteModel), Required = true, Description = "New note details.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(NoteModel), Description = "The OK response")]
        public async Task<IActionResult> CreateNote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "note/CreateNote")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger CreateNote function processed a request.");

            var authResponse = _jWTService.ValidateJWT(req);
            if (!authResponse.IsValid)
            {
                return new UnauthorizedResult();
            }

            try
            {
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                dynamic data = JsonConvert.DeserializeObject<NoteModel>(requestBody);

                var response = this.noteRL.CreateNote(data, authResponse.UserId);
                return new OkObjectResult(response);
            }
            catch (CosmosException cosmosException)
            {

                log.LogError("Creating item failed with error {0}", cosmosException.ToString());
                return new BadRequestObjectResult($"Failed to create item. Cosmos Status Code {cosmosException.StatusCode}, Sub Status Code {cosmosException.SubStatusCode}: {cosmosException.Message}.");
            }
            
        }

        [FunctionName("GetAllNotes")]
        [OpenApiOperation(operationId: "GetAllNotes", tags: new[] { "NoteService" })]
        [OpenApiSecurity("JWT Bearer Token", SecuritySchemeType.ApiKey, Name = "token", In = OpenApiSecurityLocationType.Header)]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> GetAllNotes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "GetAll")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var authResponse = _jWTService.ValidateJWT(req);
            if (!authResponse.IsValid)
            {
                return new UnauthorizedResult();
            }

            var response = await this.noteRL.GetAllNotes(authResponse.Email);

            return new OkObjectResult(response);
        }

        [FunctionName("GetNoteById")]
        [OpenApiOperation(operationId: "GetNoteById", tags: new[] { "NoteService" })]
        [OpenApiSecurity("JWT Bearer Token", SecuritySchemeType.ApiKey, Name = "token", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The id parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(NoteModel), Description = "The OK response")]
        public async Task<IActionResult> GetNoteById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "note/Get/{id}")] HttpRequest req, string id)
        {
            var authResponse = _jWTService.ValidateJWT(req);
            if (!authResponse.IsValid)
            {
                return new UnauthorizedResult();
            }

            var response = await this.noteRL.GetNoteById(authResponse.Email, id);

            return new OkObjectResult(response);
        }


        [FunctionName("UpdateNote")]
        [OpenApiOperation(operationId: "UpdateNote", tags: new[] { "NoteService" })]
        [OpenApiSecurity("JWT Bearer Token", SecuritySchemeType.ApiKey, Name = "token", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The id parameter")]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(NoteModel), Required = true, Description = "Update note details.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(NoteModel), Description = "The OK response")]
        public async Task<IActionResult> UpdateNote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "note/Update/{id}")] HttpRequest req, string id)
        {
            var authResponse = _jWTService.ValidateJWT(req);
            if (!authResponse.IsValid)
            {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<NoteModel>(requestBody);

            var response = await this.noteRL.UpdateNote(authResponse.Email, data, id);

            return new OkObjectResult(response);
        }


        [FunctionName("Pin")]
        [OpenApiOperation(operationId: "Pin", tags: new[] { "NoteService" })]
        [OpenApiSecurity("JWT Bearer Token", SecuritySchemeType.ApiKey, Name = "token", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The id parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(NoteModel), Description = "The OK response")]
        public async Task<IActionResult> Pin(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "note/Pin/{id}")] HttpRequest req, string id)
        {
            var authResponse = _jWTService.ValidateJWT(req);
            if (!authResponse.IsValid)
            {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<NoteModel>(requestBody);

            var response =  this.noteRL.Pin(authResponse.Email, id);

            return new OkObjectResult(response);
        }

        [FunctionName("Archive")]
        [OpenApiOperation(operationId: "Archive", tags: new[] { "NoteService" })]
        [OpenApiSecurity("JWT Bearer Token", SecuritySchemeType.ApiKey, Name = "token", In = OpenApiSecurityLocationType.Header)]
        [OpenApiParameter(name: "id", In = ParameterLocation.Path, Required = true, Type = typeof(string), Description = "The id parameter")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(NoteModel), Description = "The OK response")]
        public async Task<IActionResult> Archive(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "note/Archive/{id}")] HttpRequest req, string id)
        {
            var authResponse = _jWTService.ValidateJWT(req);
            if (!authResponse.IsValid)
            {
                return new UnauthorizedResult();
            }

            var response = this.noteRL.Archive(authResponse.Email, id);

            return new OkObjectResult(response);
        }
    }
}

