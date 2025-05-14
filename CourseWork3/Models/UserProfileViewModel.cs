namespace CourseWork3.Models
{
    public class UserProfileViewModel
    {
        public string? DisplayName { get; set; }
        public List<AudioFile> AudioFiles { get; set; } = [];
        public int TotalDownloads { get; set; }
        public int TotalPlays { get; set; }
    }

}
