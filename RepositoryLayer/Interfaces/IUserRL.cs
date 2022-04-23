
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

        Task<List<UserDetails>> GetUsers();

        Task<UserDetails> GetUserById(string userId);

        string ForgetPassword(ForgetPasswordDetails details);

        UserDetails ResetPassword(ResetPasswordDetails details);
    }
}
