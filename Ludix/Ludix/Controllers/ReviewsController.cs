using Ludix.Data;
using Ludix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
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
    public class ReviewsController : Controller
    {
        private readonly LudixContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewsController(LudixContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reviews
        public async Task<IActionResult> Index()
        {
            var reviews = await _context.Review
                .Include(r => r.Game)
                .Include(r => r.MyUser)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            return View(reviews);
        }

        // GET: Reviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Game)
                .Include(r => r.MyUser)
                .FirstOrDefaultAsync(m => m.ReviewId == id);

            if (review == null)
            {
                return NotFound();
            }

            return View(review);
        }

        // GET: Reviews/Create
        public async Task<IActionResult> Create(int? gameId)
        {
            // Se foi fornecido um gameId, verificar se o jogo existe
            if (gameId.HasValue)
            {
                var game = await _context.Game.FindAsync(gameId.Value);
                if (game == null)
                {
                    return NotFound();
                }
                ViewBag.PreselectedGameId = gameId.Value;
                ViewBag.PreselectedGameTitle = game.Title;
            }

            // Verificar se o utilizador já fez review para este jogo
            if (gameId.HasValue)
            {
                var currentUser = await GetCurrentUserAsync();
                if (currentUser != null)
                {
                    var existingReview = await _context.Review
                        .FirstOrDefaultAsync(r => r.GameId == gameId.Value && r.UserId == currentUser.UserId);

                    if (existingReview != null)
                    {
                        TempData["Error"] = "Já fez uma avaliação para este jogo. Pode editá-la se desejar.";
                        return RedirectToAction("Edit", new { id = existingReview.ReviewId });
                    }
                }
            }

            // Carregar jogos para dropdown (apenas jogos que o utilizador ainda não avaliou)
            var currentUserForDropdown = await GetCurrentUserAsync();
            if (currentUserForDropdown != null)
            {
                var reviewedGameIds = await _context.Review
                    .Where(r => r.UserId == currentUserForDropdown.UserId)
                    .Select(r => r.GameId)
                    .ToListAsync();

                var availableGames = await _context.Game
                    .Where(g => !reviewedGameIds.Contains(g.GameId))
                    .OrderBy(g => g.Title)
                    .ToListAsync();

                ViewData["GameId"] = new SelectList(availableGames, "GameId", "Title", gameId);
            }
            else
            {
                ViewData["GameId"] = new SelectList(_context.Game.OrderBy(g => g.Title), "GameId", "Title", gameId);
            }

            return View();
        }

        // POST: Reviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GameId,Rating,ReviewText")] Review review)
        {
            var userId = _context.MyUser
                   .FirstOrDefault(u => u.AspUser == User.FindFirstValue(ClaimTypes.NameIdentifier))?.UserId;

            if (userId == null)
                return Forbid(); // Utilizador nao autenticado corretamente

            review.UserId = userId.Value;
            review.ReviewDate = DateTime.Now;

            if (ModelState.IsValid)
            {
                review.Game = null;
                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", "Games", new { id = review.GameId });
            }

            // Reencaminhar com erro, neste caso volta ao jogo
            return RedirectToAction("Details", "Games", new { id = review.GameId });
        }


        // GET: Reviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Game)
                .Include(r => r.MyUser)
                .FirstOrDefaultAsync(r => r.ReviewId == id);

            if (review == null)
            {
                return NotFound();
            }

            // Verificar se o utilizador atual é o dono da review ou admin
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || (review.UserId != currentUser.UserId && !currentUser.IsAdmin))
            {
                return Forbid();
            }

            ViewData["GameTitle"] = review.Game.Title;
            return View(review);
        }

        // POST: Reviews/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReviewId,Rating,ReviewText,ReviewDate,UserId,GameId")] Review review)
        {
            if (id != review.ReviewId)
            {
                return NotFound();
            }

            // Verificar se o utilizador atual é o dono da review ou admin
            var currentUser = await GetCurrentUserAsync();
            var originalReview = await _context.Review.AsNoTracking().FirstOrDefaultAsync(r => r.ReviewId == id);

            if (originalReview == null)
            {
                return NotFound();
            }

            if (currentUser == null || (originalReview.UserId != currentUser.UserId && !currentUser.IsAdmin))
            {
                return Forbid();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Manter dados originais que não devem ser alterados
                    review.UserId = originalReview.UserId;
                    review.GameId = originalReview.GameId;
                    review.ReviewDate = originalReview.ReviewDate; // Manter data original

                    _context.Update(review);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Avaliação atualizada com sucesso!";
                    return RedirectToAction("Details", "Games", new { id = review.GameId });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.ReviewId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Recarregar dados para o formulário
            var game = await _context.Game.FindAsync(review.GameId);
            ViewData["GameTitle"] = game?.Title ?? "Jogo não encontrado";
            return View(review);
        }

        // GET: Reviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Review
                .Include(r => r.Game)
                .Include(r => r.MyUser)
                .FirstOrDefaultAsync(m => m.ReviewId == id);

            if (review == null)
            {
                return NotFound();
            }

            // Verificar se o utilizador atual  o dono da review ou admin
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || (review.UserId != currentUser.UserId && !currentUser.IsAdmin))
            {
                return Forbid();
            }

            return View(review);
        }

        // POST: Reviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Review.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            // Verificar se o utilizador atual e o dono da review ou admin
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null || (review.UserId != currentUser.UserId && !currentUser.IsAdmin))
            {
                return Forbid();
            }

            var gameId = review.GameId;
            _context.Review.Remove(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Avaliação removida com sucesso!";
            return RedirectToAction("Details", "Games", new { id = gameId });
        }

        // GET: Reviews/MyReviews - Lista de reviews do utilizador atual
        public async Task<IActionResult> MyReviews()
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity" });
            }

            var myReviews = await _context.Review
                .Include(r => r.Game)
                .Include(r => r.MyUser)
                .Where(r => r.UserId == currentUser.UserId)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            return View(myReviews);
        }

        // GET: Reviews/GameReviews/5 - Lista de reviews de um jogo específico
        public async Task<IActionResult> GameReviews(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game.FindAsync(id);
            if (game == null)
            {
                return NotFound();
            }

            var gameReviews = await _context.Review
                .Include(r => r.MyUser)
                .Where(r => r.GameId == id)
                .OrderByDescending(r => r.ReviewDate)
                .ToListAsync();

            ViewBag.GameTitle = game.Title;
            ViewBag.GameId = id;

            // Calcular estatísticas
            if (gameReviews.Any())
            {
                ViewBag.AverageRating = gameReviews.Average(r => r.Rating);
                ViewBag.TotalReviews = gameReviews.Count;
            }
            else
            {
                ViewBag.AverageRating = 0;
                ViewBag.TotalReviews = 0;
            }

            return View(gameReviews);
        }

        private bool ReviewExists(int id)
        {
            return _context.Review.Any(e => e.ReviewId == id);
        }

        private async Task<MyUser> GetCurrentUserAsync()
        {
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return null;
            }

            return await _context.MyUser
                .FirstOrDefaultAsync(u => u.AspUser == identityUser.Id);
        }
    }
}