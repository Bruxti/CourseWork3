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
        bool IsMusic = Request.Form["IsMusic"] == "on";

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
                Genre = IsMusic ? genre : null,
                Author = IsMusic ? author : null,
                FileName = fileName,
                OriginalName = audioFile.FileName,
                UserId = userId,
                UploadDate = DateTime.Now,
                IsMusic = IsMusic
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
    public IActionResult Stream(int id)
    {
        var file = _context.AudioFiles.Find(id);
        if (file == null) return NotFound();

        var path = Path.Combine(_environment.WebRootPath, "uploads", file.FileName);
        if (!System.IO.File.Exists(path))
            return NotFound($"Файл не найден: {path}");

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return File(stream, "audio/mpeg", enableRangeProcessing: true);
    }


    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterPlay(int id)
    {
        AudioFile? file = await _context.AudioFiles.FindAsync(id);
        if (file == null) return NotFound();

        file.PlayCount++;
        await _context.SaveChangesAsync();
        return Ok();
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
                result = result.Where(a => a.Title != null && a.Title.ToLower().Contains(query.ToLower()));
                break;
            case "genre":
                result = result.Where(a => a.Genre != null && a.Genre.ToLower().Contains(query.ToLower()));
                result = result.Where(a => !a.IsMusic);
                break;
            case "user":
                result = result.Where(a => a.User != null && a.User.DisplayName != null && a.User.DisplayName.ToLower().Contains(query.ToLower()));
                break;
            case "filename":
                result = result.Where(a => a.OriginalName != null && a.OriginalName.ToLower().Contains(query.ToLower()));
                break;
            default:
                result = Enumerable.Empty<AudioFile>().AsQueryable();
                break;
        }


        X.PagedList.IPagedList<AudioFile> pagedResult = result.OrderByDescending(a => a.Id).ToPagedList(page, pageSize);
        ViewBag.Query = query;
        ViewBag.Category = category;

        return View("SearchResults", pagedResult);
    }
    public async Task<IActionResult> Download(int id)
    {
        AudioFile? audio = await _context.AudioFiles.FindAsync(id);
        if (audio == null)
            return NotFound();

        audio.DownloadCount++;
        await _context.SaveChangesAsync();

        string path = Path.Combine(_environment.WebRootPath, "uploads", audio.FileName);
        byte[] fileBytes = await System.IO.File.ReadAllBytesAsync(path);
        return File(fileBytes, "application/octet-stream", audio.OriginalName);
    }

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        AudioFile? audio = await _context.AudioFiles
            .Include(a => a.User)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (audio == null)
            return NotFound();

        return View(audio);
    }

    [AllowAnonymous]
    public async Task<IActionResult> UserProfile(string id)
    {
        var user = await _userManager.Users
            .Include(u => u.AudioFiles)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        var totalDownloads = user.AudioFiles.Sum(f => f.DownloadCount);
        var totalPlays = user.AudioFiles.Sum(f => f.PlayCount);

        var model = new UserProfileViewModel
        {
            DisplayName = user.DisplayName,
            AudioFiles = user.AudioFiles.ToList(),
            TotalDownloads = totalDownloads,
            TotalPlays = totalPlays
        };

        return View("~/Views/User/UserProfile.cshtml", model);
    }


}
