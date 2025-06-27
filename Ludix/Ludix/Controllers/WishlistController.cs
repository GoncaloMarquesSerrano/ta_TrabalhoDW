using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using Ludix.Data;
using Ludix.Models;
using System.Linq;

namespace Ludix.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly LudixContext _context;

        public WishlistController(LudixContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private async Task<MyUser?> GetAuthenticatedUserAsync()
        {
            var userId = User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return null;

            return await _context.MyUser
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.AspUser == userId);
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
                return RedirectToAction("Login", "Account");

            var wishlistItems = await _context.WishlistItems
                .AsNoTracking()
                .Include(w => w.Game)
                .Where(w => w.UserId == user.UserId)
                .OrderByDescending(w => w.AddedDate)
                .ToListAsync();

            return View(wishlistItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int gameId, string? returnUrl = null)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = "É necessário fazer login para adicionar à wishlist.";
                return RedirectToAction("Login", "Account");
            }

            var gameExists = await _context.Game.AnyAsync(g => g.GameId == gameId);
            if (!gameExists)
            {
                TempData["ErrorMessage"] = "Jogo não encontrado.";
                return RedirectToReturnUrl(returnUrl);
            }

            var alreadyInWishlist = await _context.WishlistItems
                .AnyAsync(w => w.UserId == user.UserId && w.GameId == gameId);

            if (alreadyInWishlist)
            {
                TempData["InfoMessage"] = "Este jogo já está na sua wishlist.";
                return RedirectToReturnUrl(returnUrl);
            }

            _context.WishlistItems.Add(new WishlistItem
            {
                GameId = gameId,
                UserId = user.UserId,
                AddedDate = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Jogo adicionado à wishlist com sucesso!";
            return RedirectToReturnUrl(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id, string? returnUrl = null)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = "É necessário fazer login.";
                return RedirectToAction("Login", "Account");
            }

            var item = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.WishlistItemId == id && w.UserId == user.UserId);

            if (item == null)
            {
                TempData["ErrorMessage"] = "Item não encontrado na sua wishlist.";
                return RedirectToReturnUrl(returnUrl);
            }

            _context.WishlistItems.Remove(item);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Item removido da wishlist com sucesso!";
            return RedirectToReturnUrl(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int gameId, string? returnUrl = null)
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
            {
                TempData["ErrorMessage"] = "É necessário fazer login para usar a wishlist.";
                return RedirectToAction("Login", "Account");
            }

            var game = await _context.Game.FirstOrDefaultAsync(g => g.GameId == gameId);
            if (game == null)
            {
                TempData["ErrorMessage"] = "Jogo não encontrado.";
                return RedirectToReturnUrl(returnUrl);
            }

            var existingItem = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.UserId == user.UserId && w.GameId == gameId);

            if (existingItem != null)
            {
                _context.WishlistItems.Remove(existingItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{game.Name}' removido da wishlist.";
            }
            else
            {
                _context.WishlistItems.Add(new WishlistItem
                {
                    GameId = gameId,
                    UserId = user.UserId,
                    AddedDate = DateTime.UtcNow
                });
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"'{game.Name}' adicionado à wishlist.";
            }

            return RedirectToReturnUrl(returnUrl);
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlistCount()
        {
            var user = await GetAuthenticatedUserAsync();
            if (user == null)
                return Json(new { count = 0 });

            var count = await _context.WishlistItems
                .Where(w => w.UserId == user.UserId)
                .CountAsync();

            return Json(new { count });
        }

        private IActionResult RedirectToReturnUrl(string? returnUrl)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Games");
        }
    }
}
