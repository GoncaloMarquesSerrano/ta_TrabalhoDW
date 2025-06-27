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
        public async Task<IActionResult> Create(int gameId, int userId)
        {
            if (gameId == 0 || userId == 0)
            {
                return BadRequest("GameId e UserId são obrigatórios");
            }

            var game = await _context.Game.FindAsync(gameId);
            var user = await _context.MyUser.FindAsync(userId);

            if (game == null || user == null)
            {
                return NotFound("Jogo ou usuário não encontrado");
            }

            if (user.Balance < game.Price)
            {
                TempData["Error"] = "Saldo insuficiente para realizar a compra";
                return RedirectToAction("Details", "Games", new { id = gameId });
            }

            var purchase = new Purchase
            {
                GameId = gameId,
                UserId = userId,
                PricePaid = game.Price // Preço já definido
            };

            ViewBag.Game = game;
            ViewBag.User = user;
            ViewBag.PaymentMethods = new List<SelectListItem>
    {
        new SelectListItem { Value = "Cartão de Crédito", Text = "Cartão de Crédito" },
        new SelectListItem { Value = "PayPal", Text = "PayPal" },
        new SelectListItem { Value = "Pix", Text = "Pix" }
    };

            return View(purchase);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Purchase purchase)
        {
            Console.WriteLine($"=== DEBUG PURCHASE CREATE ===");
            Console.WriteLine($"GameId: {purchase.GameId}");
            Console.WriteLine($"UserId: {purchase.UserId}");
            Console.WriteLine($"PaymentMethod: '{purchase.PaymentMethod}'");
            Console.WriteLine($"PricePaid: {purchase.PricePaid}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

            // Garantir que os dados essenciais estejam presentes
            if (purchase.GameId == 0)
            {
                if (int.TryParse(Request.Form["GameId"], out int gameId))
                {
                    purchase.GameId = gameId;
                }
            }

            if (purchase.UserId == 0)
            {
                if (int.TryParse(Request.Form["UserId"], out int userId))
                {
                    purchase.UserId = userId;
                }
            }

            if (string.IsNullOrEmpty(purchase.PaymentMethod))
            {
                purchase.PaymentMethod = Request.Form["PaymentMethod"];
            }

            // Remover erros das propriedades de navegação
            ModelState.Remove("Game");
            ModelState.Remove("User");

            // Validação essencial
            if (purchase.GameId == 0)
            {
                ModelState.AddModelError("", "Jogo não selecionado");
                return await ReloadViewData(purchase);
            }

            if (purchase.UserId == 0)
            {
                ModelState.AddModelError("", "Usuário não identificado");
                return await ReloadViewData(purchase);
            }

            if (string.IsNullOrEmpty(purchase.PaymentMethod))
            {
                ModelState.AddModelError("PaymentMethod", "Selecione um método de pagamento");
                return await ReloadViewData(purchase);
            }

            // Buscar o jogo e usuário para validação
            var game = await _context.Game.FindAsync(purchase.GameId);
            var user = await _context.MyUser.FindAsync(purchase.UserId);

            if (game == null)
            {
                ModelState.AddModelError("", "Jogo não encontrado");
                return await ReloadViewData(purchase);
            }

            if (user == null)
            {
                ModelState.AddModelError("", "Usuário não encontrado");
                return await ReloadViewData(purchase);
            }

            if (user.Balance < game.Price)
            {
                ModelState.AddModelError("", "Saldo insuficiente para realizar a compra");
                return await ReloadViewData(purchase);
            }

            // Definir automaticamente os valores da compra
            purchase.PurchaseDate = DateTime.Now;
            purchase.PricePaid = game.Price; // Preço sempre do jogo
            purchase.Status = "Processando";

            // Salvar a compra
            _context.Add(purchase);
            await _context.SaveChangesAsync();

            // Atualizar saldo do usuário
            user.Balance -= purchase.PricePaid;
            await _context.SaveChangesAsync();

            return RedirectToAction("Confirmation", new { id = purchase.PurchaseId });
        }

// Método auxiliar para recarregar os dados da view
private async Task<IActionResult> ReloadViewData(Purchase purchase)
        {
            // Recarregar os dados do jogo e usuário
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
        public async Task<IActionResult> Checkout(int gameId)
        {
            // Obter o ID do usuário logado
            var userId = GetCurrentUserId(); // Você precisa implementar este método

            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // Reutilizar a lógica do Create
            return await Create(gameId, userId);
        }

        private int GetCurrentUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdString, out int userId) ? userId : 0;
        }
    }
}