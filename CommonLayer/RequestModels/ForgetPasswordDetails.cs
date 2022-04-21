using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLayer.RequestModels
{
    public class ForgetPasswordDetails
    {
        [JsonProperty("email")]
        public string Email { get; set; }
    }
}
