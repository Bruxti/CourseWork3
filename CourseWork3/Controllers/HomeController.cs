using CourseWork3.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using X.PagedList.Extensions;

namespace CourseWork3.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    //[Authorize]
    public async Task<IActionResult> IndexAsync(int? page)
    {
        int pageSize = 15;
        int pageNumber = page ?? 1;

        IOrderedQueryable<AudioFile> audioFiles = _context.AudioFiles
            .Include(a => a.User)
            .OrderByDescending(a => a.UploadDate);

        X.PagedList.IPagedList<AudioFile> pagedList = audioFiles.ToPagedList(pageNumber, pageSize);



        return View(pagedList);
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
