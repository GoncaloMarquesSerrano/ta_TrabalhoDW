using Ludix.Data;
using Ludix.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Ludix.Controllers
{
    [Authorize(Policy = "DeveloperOrAdmin")]
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
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var game = await _context.Game
                .Include(g => g.Developer)
                .Include(g => g.Genres)
                .Include(g => g.Purchases)
                .Include(g => g.Reviews)
                    .ThenInclude(r => r.MyUser)
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
            // Verificar se o utilizador atual é um desenvolvedor OU admin
            var currentUser = await GetCurrentUserAsync();
            var currentDeveloper = await GetCurrentDeveloperAsync();

            if (currentDeveloper == null && (currentUser == null || !currentUser.IsAdmin))
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Carregar géneros para a seleção múltipla
            ViewBag.AllGenres = await _context.Genre.ToListAsync();

            // Se é admin, carregar também todos os desenvolvedores para seleção
            if (currentUser != null && currentUser.IsAdmin)
            {
                ViewBag.AllDevelopers = await _context.Developer.ToListAsync();
            }

            return View();
        }

        // POST: Games/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create(Game game, IFormFile cover, int[] selectedGenres, int? selectedDeveloperId)
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
                var currentUser = await GetCurrentUserAsync();
                Developer developer = null;

                if (currentUser != null && currentUser.IsAdmin && selectedDeveloperId.HasValue)
                {
                    developer = await _context.Developer.FindAsync(selectedDeveloperId.Value);

                    if (developer == null)
                    {
                        ModelState.AddModelError("", "O desenvolvedor selecionado não é válido.");
                        ViewBag.AllGenres = await _context.Genre.ToListAsync();
                        ViewBag.AllDevelopers = await _context.Developer.ToListAsync();
                        return View(game);
                    }
                }
                else if (currentUser != null && currentUser.IsAdmin && !selectedDeveloperId.HasValue)
                {
                    ModelState.AddModelError("", "Como administrador, deve selecionar um desenvolvedor para o jogo.");
                    ViewBag.AllGenres = await _context.Genre.ToListAsync();
                    ViewBag.AllDevelopers = await _context.Developer.ToListAsync();
                    return View(game);
                }
                else
                {
                    var username = User.Identity?.Name;
                    developer = await _context.Developer
                        .FirstOrDefaultAsync(d => d.Email == username);
                }

                if (developer == null)
                {
                    string errorMessage = currentUser != null && currentUser.IsAdmin
                        ? "Desenvolvedor selecionado não encontrado."
                        : "A sua conta não está associada a um perfil de desenvolvedor.";

                    ModelState.AddModelError("", errorMessage);
                    ViewBag.AllGenres = await _context.Genre.ToListAsync();
                    if (currentUser != null && currentUser.IsAdmin)
                    {
                        ViewBag.AllDevelopers = await _context.Developer.ToListAsync();
                    }
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

            // Se houve erro, recarrega géneros e desenvolvedores
            ViewBag.AllGenres = await _context.Genre.ToListAsync();
            var currentUserForError = await GetCurrentUserAsync();
            if (currentUserForError != null && currentUserForError.IsAdmin)
            {
                ViewBag.AllDevelopers = await _context.Developer.ToListAsync();
            }
            return View(game);
        }

        // GET: Games/AllGames - Lista todos os jogos (público)
        [AllowAnonymous]
        public async Task<IActionResult> AllGames()
        {
            var games = await _context.Game
                .Include(g => g.Developer)
                .Include(g => g.Genres)
                .OrderByDescending(g => g.ReleaseDate)
                .ToListAsync();

            return View(games);
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

            // Verificar se o utilizador atual é o desenvolvedor deste jogo OU admin
            var currentUser = await GetCurrentUserAsync();
            var currentDeveloper = await GetCurrentDeveloperAsync();

            if ((currentDeveloper == null || game.DeveloperFk != currentDeveloper.UserId) &&
                (currentUser == null || !currentUser.IsAdmin))
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Carregar géneros para a seleção múltipla
            ViewBag.AllGenres = await _context.Genre.ToListAsync();
            ViewBag.SelectedGenres = game.Genres.Select(g => g.GenreId).ToList();

            // Se é admin, carregar também todos os desenvolvedores para seleção
            if (currentUser != null && currentUser.IsAdmin)
            {
                ViewBag.AllDevelopers = await _context.Developer.ToListAsync();
            }

            return View(game);
        }

        // POST: Games/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, Game game, IFormFile coverImage, int[] selectedGenres, int? selectedDeveloperId)
        {
            if (id != game.GameId)
            {
                return NotFound();
            }

            // Verificar se o utilizador atual é o desenvolvedor deste jogo OU admin
            var currentUser = await GetCurrentUserAsync();
            var currentDeveloper = await GetCurrentDeveloperAsync();
            var originalGame = await _context.Game.FindAsync(id);

            if (originalGame == null)
            {
                return NotFound();
            }

            bool isAuthorized = (currentDeveloper != null && originalGame.DeveloperFk == currentDeveloper.UserId) ||
                               (currentUser != null && currentUser.IsAdmin);

            if (!isAuthorized)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Se é admin e selecionou um desenvolvedor diferente
                    if (currentUser != null && currentUser.IsAdmin && selectedDeveloperId.HasValue)
                    {
                        game.DeveloperFk = selectedDeveloperId.Value;
                    }
                    else
                    {
                        // Manter o desenvolvedor original
                        game.DeveloperFk = originalGame.DeveloperFk;
                    }

                    // Processar a nova imagem se fornecida
                    if (coverImage != null && coverImage.Length > 0)
                    {
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
            if (currentUser != null && currentUser.IsAdmin)
            {
                ViewBag.AllDevelopers = await _context.Developer.ToListAsync();
            }
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

            // Verificar se o utilizador atual é o desenvolvedor deste jogo OU admin
            var currentUser = await GetCurrentUserAsync();
            var currentDeveloper = await GetCurrentDeveloperAsync();

            if ((currentDeveloper == null || game.DeveloperFk != currentDeveloper.UserId) &&
                (currentUser == null || !currentUser.IsAdmin))
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

            // Verificar se o utilizador atual é o desenvolvedor deste jogo OU admin
            var currentUser = await GetCurrentUserAsync();
            var currentDeveloper = await GetCurrentDeveloperAsync();

            bool isAdmin = currentUser != null && currentUser.IsAdmin;
            bool isGameOwner = currentDeveloper != null && game.DeveloperFk == currentDeveloper.UserId;

            if (!isAdmin && !isGameOwner)
            {
                return RedirectToAction("AccessDenied", "Account", new { area = "Identity" });
            }

            // Guardar o nome da imagem antes de fazer qualquer operação na BD
            string imageToDelete = game.Cover;

            // Tanto admins quanto developers podem apagar jogos com dependências
            // Admins podem apagar qualquer jogo, developers apenas os seus próprios
            try
            {
                // Remover todas as compras relacionadas
                var purchases = await _context.Purchase.Where(p => p.GameId == id).ToListAsync();
                if (purchases.Any())
                {
                    _context.Purchase.RemoveRange(purchases);
                }

                // Remover todas as avaliações relacionadas
                var reviews = await _context.Review.Where(r => r.GameId == id).ToListAsync();
                if (reviews.Any())
                {
                    _context.Review.RemoveRange(reviews);
                }

                // Remover associações com géneros (many-to-many)
                var gameWithGenres = await _context.Game
                    .Include(g => g.Genres)
                    .FirstOrDefaultAsync(g => g.GameId == id);

                if (gameWithGenres != null)
                {
                    gameWithGenres.Genres.Clear();
                }

                // Apagar a imagem de capa ANTES de remover o jogo da BD
                if (!string.IsNullOrEmpty(imageToDelete) && imageToDelete != "default_cover.jpg")
                {
                    // Tentar primeiro no diretório "covers"
                    string imagePath1 = Path.Combine(_webHostEnvironment.WebRootPath, "covers", imageToDelete);
                    // Tentar também no diretório "images/covers"
                    string imagePath2 = Path.Combine(_webHostEnvironment.WebRootPath, "images", "covers", imageToDelete);

                    if (System.IO.File.Exists(imagePath1))
                    {
                        try
                        {
                            System.IO.File.Delete(imagePath1);
                            Console.WriteLine($"Imagem apagada com sucesso: {imagePath1}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao apagar imagem em {imagePath1}: {ex.Message}");
                        }
                    }
                    else if (System.IO.File.Exists(imagePath2))
                    {
                        try
                        {
                            System.IO.File.Delete(imagePath2);
                            Console.WriteLine($"Imagem apagada com sucesso: {imagePath2}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Erro ao apagar imagem em {imagePath2}: {ex.Message}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Imagem não encontrada em nenhum dos caminhos: {imagePath1} ou {imagePath2}");
                    }
                }

                // Remover o registo do jogo da base de dados
                _context.Game.Remove(game);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Erro ao remover o jogo: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }

            string removalNote = "";
            if (isAdmin)
            {
                removalNote = " (remoção administrativa - todas as dependências foram removidas)";
            }
            else if (isGameOwner)
            {
                removalNote = " (todas as compras e avaliações relacionadas foram removidas)";
            }

            TempData["Message"] = $"O jogo '{game.Title}' foi excluído com sucesso{removalNote}.";
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

        // Novo método para obter o utilizador atual da tabela MyUser
        private async Task<MyUser> GetCurrentUserAsync()
        {
            // Obter o utilizador atual do Identity
            var identityUser = await _userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return null;
            }

            // Buscar o utilizador na tabela MyUser
            return await _context.MyUser
                .FirstOrDefaultAsync(u => u.AspUser == identityUser.Id);
        }
    }
}