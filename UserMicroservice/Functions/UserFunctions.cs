using System.IO;
using System.Net;
using System.Threading.Tasks;
using CommonLayer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using RepositoryLayer.Interfaces;

namespace UserMicroservice
{
    public class UserFunctions
    {
        private readonly IUserRL userRL;

        public UserFunctions(IUserRL userRL)
        {
            this.userRL = userRL;
        }

        [FunctionName("UserRegistration")]
        [OpenApiOperation(operationId: "UserRegistration", tags: new[] { "UserService" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(UserDetails), Required = true, Description = "New user details.")]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string), Description = "The OK response")]
        public async Task<IActionResult> Run(
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
    }
}

