using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonLayer.RequestModels
{
    public class NoteModel
    {
        [JsonProperty("id")]
        public string NoteId { get; set; } = "";

        public string Title { get; set; } = "";

        public string Body { get; set; } = "";

        public bool IsTrash { get; set; } = false;

        public bool IsPinned { get; set; } = false;

        public bool IsArchived { get; set; } = false;

        public List<string> Collaborations { get; set; } = new List<string>();

        public string CreatedAt { get; set; } = "";

        public string ModifiedAt { get; set; } = "";

        public string Color { get; set; } = "";

        public string BGImage { get; set; } = "";
    }
}
