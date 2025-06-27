using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ludix.Data;
using Ludix.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Ludix.Controllers
{
    [Authorize]
    public class PurchasesController : Controller
    {
        private readonly LudixContext _context;

        public PurchasesController(LudixContext context)
        {
            _context = context;
        }

        // GET: Purchases
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.MyUser.FirstOrDefaultAsync(u => u.AspUser == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var purchases = await _context.Purchase
                .Include(p => p.Game)
                .Include(p => p.User)
                .Where(p => p.UserId == user.UserId)
                .OrderByDescending(p => p.PurchaseDate)
                .ToListAsync();

            return View(purchases);
        }

        // GET: Purchases/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.MyUser.FirstOrDefaultAsync(u => u.AspUser == userId);

            var purchase = await _context.Purchase
                .Include(p => p.Game)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PurchaseId == id && m.UserId == user.UserId);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // GET: Purchases/Create
        [HttpGet]
        public async Task<IActionResult> Create(int gameId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.MyUser.FirstOrDefaultAsync(u => u.AspUser == userId);

            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var game = await _context.Game.FindAsync(gameId);
            if (game == null)
            {
                return NotFound();
            }

            // Verificar se o jogo já foi comprado
            var alreadyPurchased = await _context.Purchase
                .AnyAsync(p => p.GameId == gameId && p.UserId == user.UserId);

            if (alreadyPurchased)
            {
                TempData["Message"] = "Você já possui este jogo em sua biblioteca";
                return RedirectToAction("Details", "Games", new { id = gameId });
            }

            ViewBag.Game = game;
            ViewBag.User = user;
            ViewBag.PaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "Cartão de Crédito", Text = "Cartão de Crédito" },
                new SelectListItem { Value = "PayPal", Text = "PayPal" },
                new SelectListItem { Value = "MBay", Text = "MBay" }
            };

            return View();
        }

        // POST: Purchases/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GameId,UserId,PaymentMethod")] Purchase purchase)
        {
            if (ModelState.IsValid)
            {
                var game = await _context.Game.FindAsync(purchase.GameId);
                if (game == null)
                {
                    return NotFound();
                }

                // Configurar os dados da compra
                purchase.PurchaseDate = DateTime.Now;
                purchase.PricePaid = game.Price;
                purchase.Status = "Processando";

                _context.Add(purchase);
                await _context.SaveChangesAsync();

                // Atualizar o saldo do usuário (se necessário)
                var user = await _context.MyUser.FindAsync(purchase.UserId);
                if (user != null)
                {
                    user.Balance -= purchase.PricePaid;
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction("Confirmation", new { id = purchase.PurchaseId });
            }

            // Recarregar dados se houver erro
            ViewBag.Game = await _context.Game.FindAsync(purchase.GameId);
            ViewBag.User = await _context.MyUser.FindAsync(purchase.UserId);
            ViewBag.PaymentMethods = new List<SelectListItem>
            {
                new SelectListItem { Value = "Cartão de Crédito", Text = "Cartão de Crédito" },
                new SelectListItem { Value = "PayPal", Text = "PayPal" },
                new SelectListItem { Value = "Pix", Text = "Pix" }
            };

            return View(purchase);
        }

        // GET: Purchases/Confirmation/5
        public async Task<IActionResult> Confirmation(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.MyUser.FirstOrDefaultAsync(u => u.AspUser == userId);

            var purchase = await _context.Purchase
                .Include(p => p.Game)
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PurchaseId == id && p.UserId == user.UserId);

            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // GET: Purchases/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchase.FindAsync(id);
            if (purchase == null)
            {
                return NotFound();
            }

            ViewData["GameId"] = new SelectList(_context.Game, "GameId", "Name", purchase.GameId);
            ViewData["UserId"] = new SelectList(_context.MyUser, "UserId", "Username", purchase.UserId);
            return View(purchase);
        }

        // POST: Purchases/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("PurchaseId,PurchaseDate,PricePaid,PaymentMethod,Status,UserId,GameId")] Purchase purchase)
        {
            if (id != purchase.PurchaseId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(purchase);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PurchaseExists(purchase.PurchaseId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["GameId"] = new SelectList(_context.Game, "GameId", "Name", purchase.GameId);
            ViewData["UserId"] = new SelectList(_context.MyUser, "UserId", "Username", purchase.UserId);
            return View(purchase);
        }

        // GET: Purchases/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var purchase = await _context.Purchase
                .Include(p => p.Game)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PurchaseId == id);
            if (purchase == null)
            {
                return NotFound();
            }

            return View(purchase);
        }

        // POST: Purchases/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var purchase = await _context.Purchase.FindAsync(id);
            if (purchase != null)
            {
                _context.Purchase.Remove(purchase);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PurchaseExists(int id)
        {
            return _context.Purchase.Any(e => e.PurchaseId == id);
        }

        [HttpGet]
        public Task<IActionResult> Checkout(int gameId)
        {
            return Create(gameId); // reutiliza a lógica de Create
        }
    }
}