using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ludix.Data;
using Ludix.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Ludix.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly LudixContext _context;

        public WishlistController(LudixContext context)
        {
            _context = context;
        }

        // GET: Wishlist
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var wishlistItems = await _context.WishlistItems
                .Include(w => w.Game)
                .Where(w => w.User.AspUser == userId)
                .ToListAsync();

            return View(wishlistItems);
        }

        // POST: Wishlist/Add
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.MyUser.FirstOrDefaultAsync(u => u.AspUser == userId);

            if (user == null)
            {
                return Unauthorized();
            }

            var existingItem = await _context.WishlistItems
                .FirstOrDefaultAsync(w => w.GameId == gameId && w.UserId == user.UserId);

            if (existingItem == null)
            {
                var wishlistItem = new WishlistItem
                {
                    GameId = gameId,
                    UserId = user.UserId,
                    AddedDate = DateTime.Now
                };

                _context.Add(wishlistItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Wishlist/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int id)
        {
            var wishlistItem = await _context.WishlistItems.FindAsync(id);
            if (wishlistItem != null)
            {
                _context.WishlistItems.Remove(wishlistItem);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}