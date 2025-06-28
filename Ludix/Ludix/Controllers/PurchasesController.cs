using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ludix.Data;
using Ludix.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Ludix.Controllers
{
    public class PurchasesController : Controller
    {
        private readonly LudixContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PurchasesController(LudixContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Purchases/Buy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int id)
        {
            // Obter utilizador autenticado
            var aspUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (aspUserId == null) return Unauthorized();

            var user = await _context.MyUser.FirstOrDefaultAsync(u => u.AspUser == aspUserId);
            if (user == null) return NotFound("Utilizador não encontrado.");

            // Obter jogo
            var game = await _context.Game.FindAsync(id);
            if (game == null) return NotFound("Jogo não encontrado.");

            // Verificar se já foi comprado
            bool alreadyPurchased = await _context.Purchase.AnyAsync(p => p.UserId == user.UserId && p.GameId == id);
            if (alreadyPurchased)
            {
                TempData["ErrorMessage"] = "Já possui este jogo na sua biblioteca!";
                return RedirectToAction("Details", "Games", new { id });
            }

            // Verificar saldo
            if (user.Balance < game.Price)
            {
                TempData["ErrorMessage"] = "Saldo insuficiente para completar a compra.";
                return RedirectToAction("Details", "Games", new { id });
            }

            try
            {
                // Efetuar compra
                var purchase = new Purchase
                {
                    UserId = user.UserId,
                    GameId = game.GameId,
                    PricePaid = game.Price,
                    PurchaseDate = DateTime.Now
                };

                _context.Purchase.Add(purchase);

                // Atualizar saldo
                user.Balance -= game.Price;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Compra efetuada com sucesso! {game.Title} foi adicionado à sua biblioteca.";
                return RedirectToAction("Details", "Games", new { id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Ocorreu um erro ao processar a compra: {ex.Message}";
                return RedirectToAction("Details", "Games", new { id });
            }
        }
    }
}
