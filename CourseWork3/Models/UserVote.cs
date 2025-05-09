namespace CourseWork3.Models
{
    public class UserVote
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int AudioFileId { get; set; }

        public bool IsLike { get; set; }
    }

}
