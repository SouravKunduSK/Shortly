using Microsoft.AspNetCore.Mvc;
using Shortly.Client.Data.ViewModels;
using Shortly.Data;
using Shortly.Data.Models;
using System.Diagnostics;
using System.Security.Claims;

namespace Shortly.Client.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var newUrl = new PostUrlVM();
            return View(newUrl);
        }

        public IActionResult ShortenUrl(PostUrlVM postUrlVM)
        {
            if (ModelState.IsValid)
            {
                var loggedInUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (loggedInUserId != null)
                {
                    var newUrl = new Url()
                    {
                        OriginalLink = postUrlVM.Url,
                        ShortLink = GenerateShortUrl(6),
                        NrOfClicks = 0,
                        UserId = loggedInUserId,
                        DateCreated = DateTime.UtcNow,
                    };
                    _context.Urls.Add(newUrl);
                    _context.SaveChanges();
                    TempData["Message"] = $"Your url was shorted successfully to {newUrl.ShortLink}";

                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrMessage"] = $"You have to login first to short url.";
                    return View("Index", postUrlVM);
                }


               
            }
            return View("Index", postUrlVM);
        }

        private string GenerateShortUrl(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

            return new string(
                Enumerable.Repeat(chars, length)
                .Select(x => x[random.Next(x.Length)])
                .ToArray());
        }
    }
}