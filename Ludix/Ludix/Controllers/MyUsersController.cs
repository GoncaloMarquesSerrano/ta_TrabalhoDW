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

        // GET: Wallet/AddFunds
        public IActionResult AddFunds()
        {
            return View();
        }

        // POST: Wallet/AddFunds
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

        // POST: Wallet/PurchaseGame
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PurchaseGame(int gameId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var user = await _context.MyUser
                .Include(u => u.Purchases)
                .FirstOrDefaultAsync(u => u.AspUser == currentUserId);

            if (user == null)
            {
                return NotFound("Utilizador não encontrado.");
            }

            var game = await _context.Game.FindAsync(gameId);
            if (game == null)
            {
                return NotFound("Jogo não encontrado.");
            }

            // Verificar se o utilizador já possui o jogo
            var existingPurchase = user.Purchases.FirstOrDefault(p => p.GameId == gameId);
            if (existingPurchase != null)
            {
                TempData["ErrorMessage"] = "Já possui este jogo na sua biblioteca.";
                return RedirectToAction("Details", "Games", new { id = gameId });
            }

            // Verificar se tem saldo suficiente
            if (user.Balance < game.Price)
            {
                TempData["ErrorMessage"] = $"Saldo insuficiente. Necessário: {game.Price:C} | Disponível: {user.Balance:C}";
                return RedirectToAction("Details", "Games", new { id = gameId });
            }

            // Realizar a compra
            var purchase = new Purchase
            {
                UserId = user.UserId,
                GameId = gameId,
                PurchaseDate = DateTime.Now,
                PricePaid = game.Price
            };

            user.Balance -= game.Price;
            _context.Purchase.Add(purchase);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Jogo '{game.Title}' comprado com sucesso! Saldo restante: {user.Balance:C}";
            return RedirectToAction("Details", "Games", new { id = gameId });
        }

        // GET: Wallet/Library
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
