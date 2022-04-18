using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLayer.Models
{
    public class UserDetails
    {
        [JsonProperty("id")]
        public string UserId { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("confirmPassword")]
        public string ConfirmPassword { get; set; }

        [JsonProperty("registeredDateTime")]
        public DateTime? CreatedAt { get; set; }
    }
}
