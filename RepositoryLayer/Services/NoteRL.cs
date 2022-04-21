using CommonLayer.RequestModels;
using Microsoft.Azure.Cosmos;
using RepositoryLayer.Interfaces;
using System;
using System.Collections.Generic;
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

        public async Task<NoteModel> CreateNote(NoteModel noteModel, string email)
        {
            if(noteModel == null)
            {
                throw new NullReferenceException();
            }
            try
            {
                noteModel.CreatedAt = Convert.ToString(DateTime.Now);
                noteModel.NoteId = Guid.NewGuid().ToString();
                noteModel.Collaborations.Add(email);


                var container = this._cosmosClient.GetContainer("NoteDB", "NoteContainer");
                return await container.CreateItemAsync(noteModel, new PartitionKey(noteModel.NoteId));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
