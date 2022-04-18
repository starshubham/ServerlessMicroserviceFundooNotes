using CommonLayer.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using RepositoryLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Services
{
    public class UserRL : IUserRL
    {
        private readonly CosmosClient _cosmosClient;
        private readonly DocumentClient _client;

        public UserRL(CosmosClient cosmosClient, DocumentClient client)
        {
            this._cosmosClient = cosmosClient;
            this._client = client;
        }

        public async Task<UserDetails> CreateUser(UserDetails details)
        {
            if (details == null)
            {
                throw new NullReferenceException();
            }
            try
            {                
                var user = new UserDetails()
                {
                    Id = Guid.NewGuid().ToString(),
                    FirstName = details.FirstName,
                    LastName = details.LastName,
                    Email = details.Email,
                    Password = details.Password,
                    ConfirmPassword = details.ConfirmPassword,
                    CreatedAt = DateTime.Now
                };

                var container = this._cosmosClient.GetContainer("UserDB", "UserDetails");
                return await container.CreateItemAsync(user, new PartitionKey(user.Id.ToString()));              
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
