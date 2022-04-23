using CommonLayer.RequestModels;
using CommonLayer.ResponseModels;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using RepositoryLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Services
{
    public class UserRL : IUserRL
    {
        private readonly CosmosClient _cosmosClient;
        private readonly IJWTService _jWTService;

        public UserRL(CosmosClient cosmosClient, IJWTService jWTService)
        {
            this._cosmosClient = cosmosClient;
            this._jWTService = jWTService;
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
                    UserId = Guid.NewGuid().ToString(),
                    FirstName = details.FirstName,
                    LastName = details.LastName,
                    Email = details.Email,
                    Password = details.Password,
                    ConfirmPassword = details.ConfirmPassword,
                    CreatedAt = DateTime.Now
                };

                var container = this._cosmosClient.GetContainer("UserDB", "UserDetails");
                return await container.CreateItemAsync(user, new PartitionKey(user.UserId.ToString()));              
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public LoginResponse UserLogin(LoginCredentials userLoginDetails)
        {
            try
            {
                var container = _cosmosClient.GetContainer("UserDB", "UserDetails");
                var document = container.GetItemLinqQueryable<UserDetails>(true)
                            .Where(t => t.Email == userLoginDetails.Email && t.Password == userLoginDetails.Password)
                            .AsEnumerable().FirstOrDefault();

                if (document != null)
                {

                    LoginResponse loginResponse = new LoginResponse();
                    loginResponse.userDetails = document;
                    loginResponse.token = _jWTService.GetJWT(loginResponse.userDetails.UserId, loginResponse.userDetails.Email);
                    return loginResponse;
                }
                return null;
            }


            catch (Exception ex)
            {
                // Detect a `Resource Not Found` exception...do not treat it as error
                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message) && ex.InnerException.Message.IndexOf("Resource Not Found") != -1)
                {
                    return null;
                }
                else
                {
                    throw new Exception(ex.Message, ex);
                }
            }
        }

        public async Task<List<UserDetails>> GetUsers()
        {
            try
            {
                QueryDefinition query = new QueryDefinition("select * from UserDetails");

                var container = this._cosmosClient.GetContainer("UserDB", "UserDetails");

                List<UserDetails> userLists = new List<UserDetails>();
                using (FeedIterator<UserDetails> resultSet = container.GetItemQueryIterator<UserDetails>(query))
                {
                    while (resultSet.HasMoreResults)
                    {
                        FeedResponse<UserDetails> response = await resultSet.ReadNextAsync();

                        userLists.AddRange(response);

                    }
                    return userLists;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<UserDetails> GetUserById(string userId)
        {
            var container = this._cosmosClient.GetContainer("UserDB", "UserDetails");
            UserDetails user = await container.ReadItemAsync<UserDetails>(userId, new PartitionKey(userId));

            return user;
        }

        public string ForgetPassword(ForgetPasswordDetails details)
        {
            try
            {
                var container = this._cosmosClient.GetContainer("UserDB", "UserDetails");
                var document = container.GetItemLinqQueryable<UserDetails>(true)
                               .Where(b => b.Email == details.Email)
                               .AsEnumerable()
                               .FirstOrDefault();
                if (document != null)
                {

                    var token = _jWTService.GetJWT(document.UserId.ToString(), document.Email.ToString());
                    new MSMQ_Model().MSMQSender(token);
                    return token;
                }
                return string.Empty;
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.Message);

            }
        }

        public UserDetails ResetPassword(ResetPasswordDetails details)
        {
            if (details == null)
            {
                throw new NullReferenceException();
            }
            try
            {
                if (details.Password.Equals(details.ConfirmPassword))
                {
                    var container = this._cosmosClient.GetContainer("UserDB", "UserDetails");
                    var document = container.GetItemLinqQueryable<UserDetails>(true).Where(b => b.Email == details.Email)
                                   .AsEnumerable().FirstOrDefault();

                    if (document != null)
                    {
                        document.Email = details.Email;
                        document.Password = details.Password;
                        document.ConfirmPassword = details.ConfirmPassword;
                        document.ModifiedAt = DateTime.Now;
                        using (var response = container.ReplaceItemAsync<UserDetails>(document, document.UserId, new PartitionKey(document.UserId)))
                        {
                            return response.Result.Resource;
                        }
                            
                    }
                }
                throw new NullReferenceException();
            }
            catch (CosmosException ex)
            {
                throw new Exception(ex.Message);

            }
        }
    }
}
