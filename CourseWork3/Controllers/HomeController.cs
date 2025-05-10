using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CourseWork3.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace CourseWork3.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    //[Authorize]
    public IActionResult Index()
    {
        var audioFiles = _context.AudioFiles
            .Include(a => a.User)
            .OrderByDescending(a => a.UploadDate)
            .ToList();

        return View(audioFiles);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
