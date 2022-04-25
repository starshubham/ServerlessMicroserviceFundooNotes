using CommonLayer.RequestModels;
using Microsoft.AspNetCore.Http;
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

        Task<NoteModel> UpdateNote(string email, NoteModel updateNote, string noteId);

        bool Pin(string email, string noteId);

        bool Archive(string email, string noteId);

        bool Trash(string email, string noteId);

        bool DeleteNote(string email, string noteId);

    }
}
