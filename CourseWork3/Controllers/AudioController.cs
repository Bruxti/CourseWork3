using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.IO;
using System.Threading.Tasks;
using CourseWork3;
using CourseWork3.Models;
using System.Security.Claims;

[Authorize]
public class AudioController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _environment;

    public AudioController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _environment = env;
    }

    [HttpGet]
    public IActionResult Upload()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Upload(string title, string genre, IFormFile audioFile)
    {
        if (audioFile != null && audioFile.Length > 0)
        {
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder); // на случай, если папки ещё нет

            var fileName = Guid.NewGuid() + Path.GetExtension(audioFile.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Сохраняем файл
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            // Получаем идентификатор пользователя
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.GetUserAsync(User);
            // Создаем объект AudioFile для сохранения в базе данных
            var audio = new AudioFile
            {
                Title = title,              // Название
                Genre = genre,              // Жанр
                FileName = fileName,        // Имя файла
                OriginalName = audioFile.FileName,  // Оригинальное имя файла
                UserId = userId,
                UploadDate = DateTime.Now
            };

            _context.AudioFiles.Add(audio);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        // Если файл не был выбран
        ModelState.AddModelError("", "Файл не выбран или пуст.");
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var audioFile = await _context.AudioFiles.FindAsync(id);
        if (audioFile == null)
        {
            return NotFound(); // Если файл не найден
        }

        // Удаляем файл из файловой системы
        var filePath = Path.Combine(_environment.WebRootPath, "audio", audioFile.FileName);
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath); // Удаление файла
        }

        // Удаляем запись из базы данных
        _context.AudioFiles.Remove(audioFile);
        await _context.SaveChangesAsync();

        // После удаления перенаправляем на страницу с файлами
        return RedirectToAction("MyFiles", "Audio");
    }

    [AllowAnonymous]
    public IActionResult MyFiles()
    {
        var userId = _userManager.GetUserId(User);
        var files = _context.AudioFiles.Where(f => f.UserId == userId).ToList();
        return View(files);
    }

    public IActionResult Stream(int id)
    {
        var file = _context.AudioFiles.Find(id);
        if (file == null) return NotFound();

        var path = Path.Combine(_environment.WebRootPath, "uploads", file.FileName);
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return File(stream, "audio/mpeg");
    }
    [HttpPost]
     [HttpPost]
    public IActionResult Like(int id)
    {
        var audio = _context.AudioFiles.Find(id);
        if (audio == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var existingVote = _context.UserVotes
            .FirstOrDefault(v => v.UserId == userId && v.AudioFileId == id);

        if (existingVote == null)
        {
            audio.Likes++;
            _context.UserVotes.Add(new UserVote
            {
                UserId = userId,
                AudioFileId = id,
                IsLike = true
            });
        }
        else if (!existingVote.IsLike)
        {
            audio.Likes++;
            audio.Dislikes--;
            existingVote.IsLike = true;
        }

        _context.SaveChanges();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public IActionResult Dislike(int id)
    {
        var audio = _context.AudioFiles.Find(id);
        if (audio == null) return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var existingVote = _context.UserVotes
            .FirstOrDefault(v => v.UserId == userId && v.AudioFileId == id);

        if (existingVote == null)
        {
            audio.Dislikes++;
            _context.UserVotes.Add(new UserVote
            {
                UserId = userId,
                AudioFileId = id,
                IsLike = false
            });
        }
        else if (existingVote.IsLike)
        {
            audio.Likes--;
            audio.Dislikes++;
            existingVote.IsLike = false;
        }

        _context.SaveChanges();
        return RedirectToAction("Index", "Home");
    }

}
