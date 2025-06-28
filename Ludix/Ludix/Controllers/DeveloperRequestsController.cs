using System;
using System.Linq;
using System.Threading.Tasks;
using Ludix.Data;
using Ludix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ludix.Controllers
{
    [Authorize]
    public class DeveloperRequestsController : Controller
    {
        private readonly LudixContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DeveloperRequestsController(LudixContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /DeveloperRequests
        [Authorize]
        public async Task<IActionResult> Index()
        {
            // Obter o utilizador atual
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null)
            {
                return NotFound();
            }

            // Verificar se  um admin
            if (!currentUser.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Obter as solicitacoes pendentes
            var pendingRequests = await _context.MyUser
                .Where(u => u.RequestedDeveloper && !_context.Developer.Any(d => d.UserId == u.UserId))
                .ToListAsync();

            return View(pendingRequests);
        }

        // GET: /DeveloperRequests/Approve/5
        [Authorize]
        public async Task<IActionResult> Approve(int id)
        {
            // Obter o utilizador atual
            var currentAdmin = await GetCurrentUserAsync();
            if (currentAdmin == null || !currentAdmin.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Obter o utilizador solicitante
            var requestingUser = await _context.MyUser
                .FirstOrDefaultAsync(u => u.UserId == id && u.RequestedDeveloper);

            if (requestingUser == null)
            {
                return NotFound();
            }

            return View(requestingUser);
        }

        // POST: /DeveloperRequests/Approve/5
        [HttpPost, ActionName("Approve")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> ApproveConfirmed(int id)
        {
            // Obter o utilizador atual
            var currentAdmin = await GetCurrentUserAsync();
            if (currentAdmin == null || !currentAdmin.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Obter o utilizador solicitante usando AsNoTracking 
            var requestingUser = await _context.MyUser.AsNoTracking()
                .FirstOrDefaultAsync(u => u.UserId == id && u.RequestedDeveloper);

            if (requestingUser == null)
            {
                return NotFound();
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Criar um novo desenvolvedor baseado no utilizador existente
                    var developer = new Developer
                    {
                        UserId = requestingUser.UserId,
                        AspUser = requestingUser.AspUser,
                        Email = requestingUser.Email,
                        Username = requestingUser.Username,
                        Balance = requestingUser.Balance,
                        CreatedAt = requestingUser.CreatedAt,
                        IsAdmin = requestingUser.IsAdmin,
                        Website = requestingUser.ProposedWebsite,
                        ApprovalDate = DateTime.Now,
                        ApprovedByUserId = currentAdmin.UserId
                    };

                    // Remover o utilizador original
                    var originalUser = await _context.MyUser.FindAsync(id);
                    _context.MyUser.Remove(originalUser);

                    // Adicionar o novo desenvolvedor
                    _context.Developer.Add(developer);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    TempData["Message"] = $"O utilizador {developer.Username} foi aprovado como desenvolvedor.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    ModelState.AddModelError(string.Empty, $"Erro ao aprovar o desenvolvedor: {ex.Message}");
                    return View(requestingUser);
                }
            }
        }

        // GET: /DeveloperRequests/Reject/5
        [Authorize]
        public async Task<IActionResult> Reject(int id)
        {
            // Obter o utilizador atual
            var currentAdmin = await GetCurrentUserAsync();
            if (currentAdmin == null || !currentAdmin.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Obter o utilizador solicitante
            var requestingUser = await _context.MyUser
                .FirstOrDefaultAsync(u => u.UserId == id && u.RequestedDeveloper);

            if (requestingUser == null)
            {
                return NotFound();
            }

            return View(requestingUser);
        }

        // POST: /DeveloperRequests/Reject/5
        [HttpPost, ActionName("Reject")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> RejectConfirmed(int id)
        {
            // Obter o utilizador atual
            var currentAdmin = await GetCurrentUserAsync();
            if (currentAdmin == null || !currentAdmin.IsAdmin)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Obter o utilizador solicitante
            var requestingUser = await _context.MyUser.FindAsync(id);
            if (requestingUser == null)
            {
                return NotFound();
            }

            // Remover a solicitacao
            requestingUser.RequestedDeveloper = false;
            requestingUser.ProposedWebsite = null;
            requestingUser.DeveloperRequestDate = null;

            await _context.SaveChangesAsync();
            TempData["Message"] = $"A solicitação de {requestingUser.Username} para se tornar desenvolvedor foi rejeitada.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<MyUser> GetCurrentUserAsync()
        {
            // Obter o utilizador atual do Identity
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            // Obter o MyUser correspondente
            return await _context.MyUser
                .FirstOrDefaultAsync(u => u.AspUser == user.Id);
        }
    }
}