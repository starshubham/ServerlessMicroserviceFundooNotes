using CommonLayer.RequestModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryLayer.Interfaces
{
    public interface INoteRL
    {
        NoteModel CreateNote(NoteModel noteModel, string userId);

        Task<List<NoteModel>> GetAllNotes(string email);

        Task<NoteModel> GetNoteById(string email, string noteId);
    }
}
