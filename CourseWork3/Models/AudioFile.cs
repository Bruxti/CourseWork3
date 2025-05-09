using System.ComponentModel.DataAnnotations;

namespace CourseWork3.Models
{
    public class AudioFile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // от Identity

        [Required]
        public string FileName { get; set; } // имя на сервере

        [Required]
        public string OriginalName { get; set; } // имя, как загрузил пользователь

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public string? Title { get; set; }

        public string? Genre { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }
}
