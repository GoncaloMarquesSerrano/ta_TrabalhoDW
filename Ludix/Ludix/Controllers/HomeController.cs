using Ludix.Data;
using Ludix.Models;
using Ludix.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Ludix.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly LudixContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(ILogger<HomeController> logger, LudixContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            
            var featuredGames = await _context.Game
                .Include(g => g.Reviews)
                .Include(g => g.Developer)
                .Include(g => g.Genres)
                .OrderByDescending(g => g.Reviews.Average(r => r.Rating))
                .ThenByDescending(g => g.Reviews.Count)
                .Take(6) 
                .ToListAsync();

            // Se nao houver jogos com reviews os mais recentes
            if (!featuredGames.Any())
            {
                featuredGames = await _context.Game
                    .Include(g => g.Reviews)
                    .Include(g => g.Developer)
                    .Include(g => g.Genres)
                    .OrderByDescending(g => g.ReleaseDate)
                    .Take(6)
                    .ToListAsync();
            }

            return View(featuredGames);
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
}