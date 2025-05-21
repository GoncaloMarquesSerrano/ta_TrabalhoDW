using Ludix.Data;
using Ludix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Ludix.Controllers
{
    [Authorize]
    public class GamesController : Controller
    {
        private readonly LudixContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public GamesController(
            LudixContext context,
            UserManager<IdentityUser> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Games
        public async Task<IActionResult> Index()
        {
            var games = await _context.Game
                .Include(g => g.Developer)
                .OrderByDescending(g => g.ReleaseDate)
                .ToListAsync();

            return View(games);
        }

        // GET: Games/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game
                .Include(g => g.Developer)
                .Include(g => g.Genres)
                .Include(g => g.Reviews)
                .FirstOrDefaultAsync(m => m.GameId == id);

            if (game == null)
            {
                return NotFound();
            }

            return View(game);
        }

        // GET: Games/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            // Verificar se o utilizador atual é um desenvolvedor
            var currentDeveloper = await GetCurrentDeveloperAsync();
            if (currentDeveloper == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Carregar géneros para a seleção múltipla
            ViewBag.AllGenres = await _context.Genre.ToListAsync();

            return View();
        }

        // POST: Games/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Game game, IFormFile cover, int[] selectedGenres)
        {
            bool hasError = false;
            string imageName = "";

            // Verificar imagem
            if (cover == null)
            {
                ModelState.AddModelError("", "É necessário fornecer uma capa.");
                hasError = true;
            }
            else if (cover.ContentType != "image/jpeg" && cover.ContentType != "image/png")
            {
                ModelState.AddModelError("", "A capa tem de ser uma imagem JPG ou PNG.");
                hasError = true;
            }
            else
            {
                // Criar nome único
                Guid g = Guid.NewGuid();
                string extension = Path.GetExtension(cover.FileName).ToLowerInvariant();
                imageName = g.ToString() + extension;

                // Guardar nome na propriedade Cover (coluna da BD)
                game.Cover = imageName;
            }

            ModelState.Remove("Cover");

            if (ModelState.IsValid && !hasError)
            {
                // Procurar utilizador autenticado
                var username = User.Identity?.Name;
                var developer = await _context.Developer
                    .FirstOrDefaultAsync(d => d.Email == username);

                if (developer == null)
                {
                    ModelState.AddModelError("", "Developer não encontrado.");
                    ViewBag.AllGenres = await _context.Genre.ToListAsync();
                    return View(game);
                }

                // Associar developer
                game.DeveloperFk = developer.UserId;

                // Associar géneros
                game.Genres = await _context.Genre
                    .Where(g => selectedGenres.Contains(g.GenreId))
                    .ToListAsync();

                // Adicionar e guardar
                _context.Add(game);
                await _context.SaveChangesAsync();

                // Guardar ficheiro no servidor
                string path = Path.Combine(_webHostEnvironment.WebRootPath, "covers");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                string fullPath = Path.Combine(path, imageName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await cover.CopyToAsync(stream);
                }

                return RedirectToAction(nameof(Index));
            }

            // Se houve erro, recarrega géneros
            ViewBag.AllGenres = await _context.Genre.ToListAsync();
            return View(game);
        }
        // GET: Games/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Incluir os géneros para edição
            var game = await _context.Game
                .Include(g => g.Genres)
                .FirstOrDefaultAsync(g => g.GameId == id);

            if (game == null)
            {
                return NotFound();
            }

            // Verificar se o utilizador atual é o desenvolvedor deste jogo
            var currentDeveloper = await GetCurrentDeveloperAsync();
            if (currentDeveloper == null || game.DeveloperFk != currentDeveloper.UserId)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Carregar géneros para a seleção múltipla
            ViewBag.AllGenres = await _context.Genre.ToListAsync();
            ViewBag.SelectedGenres = game.Genres.Select(g => g.GenreId).ToList();

            return View(game);
        }

        // POST: Games/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Game game, IFormFile coverImage, int[] selectedGenres)
        {
            if (id != game.GameId)
            {
                return NotFound();
            }

            // Verificar se o utilizador atual é o desenvolvedor deste jogo
            var currentDeveloper = await GetCurrentDeveloperAsync();
            var originalGame = await _context.Game.FindAsync(id);

            if (currentDeveloper == null || originalGame == null || originalGame.DeveloperFk != currentDeveloper.UserId)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Manter o desenvolvedor original
                    game.DeveloperFk = originalGame.DeveloperFk;

                    // Processar a nova imagem se fornecida
                    if (coverImage != null && coverImage.Length > 0)
                    {
                        // NÃO APAGAR a imagem antiga

                        // Guardar a nova imagem
                        string uniqueFileName = Guid.NewGuid().ToString() + "_" + Path.GetFileName(coverImage.FileName);
                        string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "covers");

                        // Criar diretório se não existir
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await coverImage.CopyToAsync(fileStream);
                        }

                        game.Cover = uniqueFileName;
                    }
                    else
                    {
                        // Manter a imagem original
                        game.Cover = originalGame.Cover;
                    }

                    // Atualizar o jogo
                    _context.Entry(originalGame).State = EntityState.Detached;
                    _context.Update(game);

                    // Atualizar géneros
                    var gameToUpdate = await _context.Game
                        .Include(g => g.Genres)
                        .FirstOrDefaultAsync(g => g.GameId == id);

                    gameToUpdate.Genres.Clear();

                    if (selectedGenres != null && selectedGenres.Length > 0)
                    {
                        foreach (var genreId in selectedGenres)
                        {
                            var genre = await _context.Genre.FindAsync(genreId);
                            if (genre != null)
                            {
                                gameToUpdate.Genres.Add(genre);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();

                    TempData["Message"] = $"O jogo '{game.Title}' foi atualizado com sucesso.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GameExists(game.GameId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Se chegamos aqui algo correu mal
            ViewBag.AllGenres = await _context.Genre.ToListAsync();
            ViewBag.SelectedGenres = selectedGenres ?? Array.Empty<int>();
            return View(game);
        }


        // GET: Games/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game
                .Include(g => g.Developer)
                .FirstOrDefaultAsync(m => m.GameId == id);

            if (game == null)
            {
                return NotFound();
            }

            // Verificar se o utilizador atual é o desenvolvedor deste jogo
            var currentDeveloper = await GetCurrentDeveloperAsync();
            if (currentDeveloper == null || game.DeveloperFk != currentDeveloper.UserId)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            return View(game);
        }

        // POST: Games/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var game = await _context.Game.FindAsync(id);

            if (game == null)
            {
                return NotFound();
            }

            // Verificar se o utilizador atual é o desenvolvedor deste jogo
            var currentDeveloper = await GetCurrentDeveloperAsync();
            if (currentDeveloper == null || game.DeveloperFk != currentDeveloper.UserId)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Verificar se existem compras ou avaliações para este jogo
            bool hasRelatedData = await _context.Purchase.AnyAsync(p => p.GameId == id) ||
                                  await _context.Review.AnyAsync(r => r.GameId == id);

            if (hasRelatedData)
            {
                TempData["Error"] = "Não é possível excluir este jogo porque existem compras ou avaliações associadas a ele.";
                return RedirectToAction(nameof(Index));
            }

            // Apagar a imagem de capa se não for a padrão
            if (!string.IsNullOrEmpty(game.Cover) && game.Cover != "default_cover.jpg")
            {
                string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "covers", game.Cover);
                if (System.IO.File.Exists(imagePath))
                {
                    try
                    {
                        System.IO.File.Delete(imagePath);
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "Não foi possível apagar o jogo");
                        return RedirectToAction("Index");
                    }
                }
            }

            // Remover o registo
            _context.Game.Remove(game);
            await _context.SaveChangesAsync();

            TempData["Message"] = $"O jogo '{game.Title}' foi excluído com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Games/MyGames - Lista de jogos do desenvolvedor atual
        [Authorize]
        public async Task<IActionResult> MyGames()
        {
            // Verificar se o utilizador atual é um desenvolvedor
            var currentDeveloper = await GetCurrentDeveloperAsync();
            if (currentDeveloper == null)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Obter os jogos do desenvolvedor atual
            var myGames = await _context.Game
                .Where(g => g.DeveloperFk == currentDeveloper.UserId)
                .Include(g => g.Genres)
                .OrderByDescending(g => g.ReleaseDate)
                .ToListAsync();


            return View(myGames);
        }

        private bool GameExists(int id)
        {
            return _context.Game.Any(e => e.GameId == id);
        }

        private async Task<Developer> GetCurrentDeveloperAsync()
        {
            // Obter o utilizador atual do Identity
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return null;
            }

            // Verificar se é um desenvolvedor
            return await _context.Developer
                .FirstOrDefaultAsync(d => d.AspUser == user.Id);
        }
    }
}