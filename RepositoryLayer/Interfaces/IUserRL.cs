
using CommonLayer.RequestModels;
using CommonLayer.ResponseModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Interfaces
{
    public interface IUserRL
    {
        Task<UserDetails> CreateUser(UserDetails details);

        LoginResponse UserLogin(LoginCredentials userLoginDetails);

    }
}
