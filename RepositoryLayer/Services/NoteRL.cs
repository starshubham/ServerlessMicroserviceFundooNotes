using CommonLayer.RequestModels;
using Microsoft.Azure.Cosmos;
using RepositoryLayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Services
{
    public class NoteRL : INoteRL
    {
        private readonly CosmosClient _cosmosClient;

        public NoteRL(CosmosClient cosmosClient)
        {
            this._cosmosClient = cosmosClient;
        }

        public NoteModel CreateNote(NoteModel noteModel, string userId)
        {
            if (noteModel == null)
            {
                throw new NullReferenceException();
            }
            try
            {
                var note = new NoteModel()
                {
                    NoteId = Guid.NewGuid().ToString(),
                    Title = noteModel.Title,
                    Description = noteModel.Description,
                    Color = noteModel.Color,
                    BGImage = noteModel.BGImage,
                    IsArchive = noteModel.IsArchive,
                    IsPin = noteModel.IsPin,
                    IsTrash = noteModel.IsTrash,
                    Reminder = DateTime.Now,
                    CreatedAt = DateTime.Now,

                };

                var container = this._cosmosClient.GetContainer("NoteDB", "NoteContainer");
                using (var result = container.CreateItemAsync(note, new PartitionKey(note.NoteId.ToString())))
                {
                    return result.Result.Resource;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<NoteModel>> GetAllNotes(string email)
        {
            try
            {
                QueryDefinition query = new QueryDefinition("select * from NoteContainer");

                var container = this._cosmosClient.GetContainer("NoteDB", "NoteContainer");

                List<NoteModel> noteLists = new List<NoteModel>();
                using (FeedIterator<NoteModel> resultSet = container.GetItemQueryIterator<NoteModel>(query))
                {
                    while (resultSet.HasMoreResults)
                    {
                        FeedResponse<NoteModel> response = await resultSet.ReadNextAsync();

                        noteLists.AddRange(response);

                    }
                    return noteLists;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<NoteModel> GetNoteById(string email, string noteId)
        {
            var container = this._cosmosClient.GetContainer("NoteDB", "NoteContainer");
            NoteModel note = await container.ReadItemAsync<NoteModel>(noteId, new PartitionKey(noteId));

            return note;
        }

        public async Task<NoteModel> UpdateNote(string email, NoteModel updateNote, string noteId)
        {
            var container = this._cosmosClient.GetContainer("NoteDB", "NoteContainer");
            var document = container.GetItemLinqQueryable<NoteModel>(true).Where(b => b.NoteId == noteId)
                            .AsEnumerable().FirstOrDefault();

            if (document != null)
            {
                document.Title = updateNote.Title;
                document.Description = updateNote.Description;
                document.Color = updateNote.Color;
                document.BGImage = updateNote.BGImage;
                document.ModifiedAt = DateTime.Now;
                var response = await container.ReplaceItemAsync<NoteModel>(document, document.NoteId, new PartitionKey(document.NoteId));
                return response.Resource;                
            }
            return null;
        }

        public bool Pin(string email, string noteId)
        {
            var container = this._cosmosClient.GetContainer("NoteDB", "NoteContainer");
            var document = container.GetItemLinqQueryable<NoteModel>(true).Where(b => b.NoteId == noteId)
                            .AsEnumerable().FirstOrDefault();

            if (document.IsPin == true)
            {
                document.IsPin = false;
                container.ReplaceItemAsync<NoteModel>(document, document.NoteId, new PartitionKey(document.NoteId));
            }
            return true;
        }

        public bool Archive(string email, string noteId)
        {
            var container = this._cosmosClient.GetContainer("NoteDB", "NoteContainer");
            var document = container.GetItemLinqQueryable<NoteModel>(true).Where(b => b.NoteId == noteId)
                            .AsEnumerable().FirstOrDefault();

            if (document.IsArchive == true)
            {
                document.IsArchive = false;
                container.ReplaceItemAsync<NoteModel>(document, document.NoteId, new PartitionKey(document.NoteId));
            }
            return true;
        }
    }
}
