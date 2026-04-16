using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http; // IFormFile için şart

namespace MezuniyetPlatformu.Web.ViewModels
{
    // Listeleme ve Detay için kullanılan model
    public class ExperiencePostViewModel
    {
        public int PostId { get; set; }
        public int AuthorUserId { get; set; }
        public UserViewModel AuthorUser { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string? ImageUrl { get; set; } // Resim Yolu
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int LikeCount { get; set; } // Toplam beğeni sayısı
        public bool IsLikedByCurrentUser { get; set; } // O anki kullanıcı beğendi mi?
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>(); // Yorumlar listesi
    }
    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public string UserName { get; set; }
        public string CommentText { get; set; }
        public DateTime CommentDate { get; set; }
    }
}