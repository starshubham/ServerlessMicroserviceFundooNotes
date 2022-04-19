using CommonLayer.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLayer.ResponseModels
{
    public class LoginResponse
    {
        public UserDetails userDetails { get; set; }

        public string token { get; set; }
    }
}
