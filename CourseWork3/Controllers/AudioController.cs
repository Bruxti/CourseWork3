using CourseWork3.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using X.PagedList.Extensions;

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
    public async Task<IActionResult> Upload(string title, string genre, string author, IFormFile audioFile)
    {
        if (audioFile != null && audioFile.Length > 0)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid() + Path.GetExtension(audioFile.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (FileStream stream = new FileStream(filePath, FileMode.Create))
            {
                await audioFile.CopyToAsync(stream);
            }

            string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ApplicationUser? user = await _userManager.GetUserAsync(User);

            AudioFile audio = new AudioFile
            {
                Title = title,
                Genre = genre,
                Author = author,
                FileName = fileName,
                OriginalName = audioFile.FileName,
                UserId = userId,
                UploadDate = DateTime.Now
            };

            _context.AudioFiles.Add(audio);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
        ModelState.AddModelError("", "Файл не выбран или пуст.");
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        AudioFile? audioFile = await _context.AudioFiles.FindAsync(id);
        if (audioFile == null)
            return NotFound();
        
        string filePath = Path.Combine(_environment.WebRootPath, "audio", audioFile.FileName);
        if (System.IO.File.Exists(filePath))
            System.IO.File.Delete(filePath);

        _context.AudioFiles.Remove(audioFile);
        await _context.SaveChangesAsync();
        return RedirectToAction("MyFiles", "Audio");
    }

    [Authorize]
    public IActionResult MyFiles()
    {
        string? userId = _userManager.GetUserId(User);
        List<AudioFile> files = _context.AudioFiles
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Id)
            .ToList();

        return View(files);
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        AudioFile? audio = await _context.AudioFiles.FindAsync(id);
        if (audio == null || audio.UserId != _userManager.GetUserId(User))
            return NotFound();

        return View(audio);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string title, string genre, string author, string originalName)
    {
        AudioFile? audio = await _context.AudioFiles.FindAsync(id);
        if (audio == null || audio.UserId != _userManager.GetUserId(User))
            return NotFound();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(originalName))
        {
            ModelState.AddModelError("", "Название и оригинальное имя обязательны.");
            return View(audio);
        }

        audio.Title = title;
        audio.Genre = genre;
        audio.Author = author;
        audio.OriginalName = originalName;

        await _context.SaveChangesAsync();

        return RedirectToAction("MyFiles");
    }




    [AllowAnonymous]
    public async Task<IActionResult> Stream(int id)
    {
        AudioFile? file = _context.AudioFiles.Find(id);
        if (file == null) return NotFound();
        file.PlayCount++;
        await _context.SaveChangesAsync();
        string path = Path.Combine(_environment.WebRootPath, "uploads", file.FileName);
        FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return File(stream, "audio/mpeg");
    }

    [HttpPost]
    public IActionResult Like(int id)
    {
        AudioFile? audio = _context.AudioFiles.Find(id);
        if (audio == null) return NotFound();

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        UserVote? existingVote = _context.UserVotes
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
        AudioFile? audio = _context.AudioFiles.Find(id);
        if (audio == null) return NotFound();

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        UserVote? existingVote = _context.UserVotes
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

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Search()
    {
        return View();
    }

    [AllowAnonymous]
    [HttpPost]
    public IActionResult Search(string query, string category, int page = 1, int pageSize = 15)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(category))
            return View("SearchResults", Enumerable.Empty<AudioFile>().ToPagedList(page, pageSize));

        IQueryable<AudioFile> result = _context.AudioFiles.Include(a => a.User);

        switch (category.ToLower())
        {
            case "title":
                result = result.Where(a => a.Title.ToLower().Contains(query.ToLower()));
                break;
            case "genre":
                result = result.Where(a => a.Genre.ToLower().Contains(query.ToLower()));
                break;
            case "user":
                result = result.Where(a => a.User.DisplayName.ToLower().Contains(query.ToLower()));
                break;
            case "filename":
                result = result.Where(a => a.OriginalName.ToLower().Contains(query.ToLower()));
                break;
            default:
                result = Enumerable.Empty<AudioFile>().AsQueryable();
                break;
        }

        var pagedResult = result.OrderByDescending(a => a.Id).ToPagedList(page, pageSize);
        ViewBag.Query = query;
        ViewBag.Category = category;

        return View("SearchResults", pagedResult);
    }
    public async Task<IActionResult> Download(int id)
    {
        var audio = await _context.AudioFiles.FindAsync(id);
        if (audio == null)
            return NotFound();

        audio.DownloadCount++;
        await _context.SaveChangesAsync();

        var path = Path.Combine(_environment.WebRootPath, "uploads", audio.FileName);
        var fileBytes = await System.IO.File.ReadAllBytesAsync(path);
        return File(fileBytes, "application/octet-stream", audio.OriginalName);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var audio = await _context.AudioFiles
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (audio == null)
            return NotFound();

        return View(audio);
    }



}
