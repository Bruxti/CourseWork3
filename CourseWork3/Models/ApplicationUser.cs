using CourseWork3.Models;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; }
    public ICollection<AudioFile> AudioFiles { get; set; }
}
