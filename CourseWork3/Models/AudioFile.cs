using System.ComponentModel.DataAnnotations;

namespace CourseWork3.Models
{
    public class AudioFile
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }   

        [Required]
        public string FileName { get; set; } 

        [Required]
        public string OriginalName { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;

        public string? Title { get; set; }

        public string? Genre { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }
    }
}
