using System.IO;
using System.Net;
using System.Threading.Tasks;
using CommonLayer.RequestModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "note/CreateNote")] HttpRequest req)
        {
            var authResponse = _jWTService.ValidateJWT(req);
            if (!authResponse.IsValid)
            {
                return new UnauthorizedResult();
            }

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject<NoteModel>(requestBody);

            var response = this.noteRL.CreateNote(data, authResponse.Email);
            return new OkObjectResult(response);
        }
    }
}

