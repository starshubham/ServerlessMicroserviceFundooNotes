using CommonLayer.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Services
{
    public class UserRL
    {
        private readonly CosmosClient _cosmosClient;
        private readonly DocumentClient _client;

        public UserRL(CosmosClient cosmosClient, DocumentClient client)
        {
            this._cosmosClient = cosmosClient;
            this._client = client;
        }

        public async Task CreateUser(UserDetails details)
        {
            try
            {
                if (details != null)
                {
                    var user = new UserDetails()
                    {
                        Id = details.Id,
                        FirstName = details.FirstName,
                        LastName = details.LastName,
                        Email = details.Email,
                        Password = details.Password,
                        ConfirmPassword = details.ConfirmPassword,
                        CreatedAt = DateTime.Now
                    };

                    var container = this._cosmosClient.GetContainer("UserDB", "UserDetails");
                    var result = await container.CreateItemAsync(user, new PartitionKey(user.Id.ToString()));
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
