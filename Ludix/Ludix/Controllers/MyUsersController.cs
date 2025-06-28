using Ludix.Data;
using Ludix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Ludix.Controllers
{
    [Authorize]
    public class MyUsersController : Controller
    {
        private readonly LudixContext _context;

        public MyUsersController(LudixContext context)
        {
            _context = context;
        }

        // GET: MyUsers
        public async Task<IActionResult> Index()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.MyUser
                .Include(u => u.Purchases)
                    .ThenInclude(p => p.Game)
                .FirstOrDefaultAsync(u => u.AspUser == currentUserId);

            if (user == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            return View(user);
        }

        // GET: MyUsers/AddFunds
        public IActionResult AddFunds()
        {
            return View();
        }

        // POST: MyUsers/AddFunds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFunds(decimal amount)
        {
            if (amount <= 0)
            {
                ModelState.AddModelError("", "O valor deve ser superior a zero.");
                return View();
            }

            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.MyUser.FirstOrDefaultAsync(u => u.AspUser == currentUserId);

            if (user == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            user.Balance += amount;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Saldo adicionado com sucesso! Novo saldo: {user.Balance:C}";
            return RedirectToAction(nameof(Index));
        }


        // GET: MyUsers/Library
        public async Task<IActionResult> Library()
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.MyUser
                .Include(u => u.Purchases)
                    .ThenInclude(p => p.Game)
                        .ThenInclude(g => g.Genres)
                .FirstOrDefaultAsync(u => u.AspUser == currentUserId);

            if (user == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            var purchasedGames = user.Purchases.OrderByDescending(p => p.PurchaseDate).ToList();
            return View(purchasedGames);
        }
    }
}
