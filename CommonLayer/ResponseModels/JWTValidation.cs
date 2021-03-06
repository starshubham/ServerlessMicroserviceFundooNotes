using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLayer.ResponseModels
{
    public class JWTValidation
    {
        public bool IsValid { get; set; }

        [JsonProperty("id")]
        public string UserId { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
