using CommonLayer.ResponseModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace RepositoryLayer.Interfaces
{
    public interface IJWTService
    {
        string GetJWT(string userId, string email);

        public JWTValidation ValidateJWT(HttpRequest request);

    }
}
