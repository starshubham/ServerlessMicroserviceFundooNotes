using CommonLayer.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Interfaces
{
    public interface INoteRL
    {
        Task<NoteModel> CreateNote(NoteModel noteModel, string email);
    }
}
