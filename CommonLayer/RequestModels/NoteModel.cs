using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLayer.RequestModels
{
    public class NoteModel
    {
        [JsonProperty("id", ReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public string NoteId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("bgImage")]
        public string BGImage { get; set; }

        [JsonProperty("isTrash")]
        public bool IsTrash { get; set; }

        [JsonProperty("isPin")]
        public bool IsPin { get; set; }

        [JsonProperty("isArchive")]
        public bool IsArchive { get; set; }

        [JsonProperty("reminder")]
        public DateTime? Reminder { get; set; }

        [JsonProperty("createdAt")]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("modifiedAt")]
        public DateTime? ModifiedAt { get; set; }

        
    }
}
