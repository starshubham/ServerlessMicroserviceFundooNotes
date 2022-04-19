using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLayer.RequestModels
{
    public class UserDetails
    {
        [JsonProperty("id")]
        public string UserId { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Password")]
        public string Password { get; set; }

        [JsonProperty("confirmPassword")]
        public string ConfirmPassword { get; set; }

        [JsonProperty("registeredDateTime")]
        public DateTime? CreatedAt { get; set; }
    }
}
