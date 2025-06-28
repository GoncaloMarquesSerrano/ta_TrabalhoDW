// UsersController.cs - Atualizado para JWT
using Ludix.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = "Bearer")] 
public class MyUsersController : ControllerBase
{
    private readonly LudixContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public MyUsersController(LudixContext context, UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: api/MyUsers/profile/{id}
    [HttpGet("profile/{id:int}")]
    public async Task<IActionResult> GetUserProfileById(int id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId == null)
        {
            return Unauthorized(new { error = "Utilizador não autenticado" });
        }

        // Vai buscar o utilizador autenticado
        var currentUser = await _context.MyUser
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.AspUser == currentUserId);

        if (currentUser == null)
        {
            return NotFound(new { error = "Utilizador autenticado não encontrado" });
        }

        // Vai buscar o utilizador alvo
        var targetUser = await _context.MyUser
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (targetUser == null)
        {
            return NotFound(new { error = "Utilizador alvo não encontrado" });
        }

        var aspUser = await _userManager.FindByIdAsync(targetUser.AspUser);

        if (aspUser == null)
        {
            return NotFound(new { error = "Utilizador ASP.NET não encontrado" });
        }

        // Só pode ver se for admin ou se for o próprio utilizador
        if (!currentUser.IsAdmin && targetUser.AspUser != currentUser.AspUser)
        {
            return Forbid();
        }

        var dto = new MyUserDTO
        {
            UserId = targetUser.UserId,
            Username = aspUser.UserName ?? "Sem nome",
            Email = aspUser.Email ?? "Sem email",
            CreatedAt = targetUser.CreatedAt,
            IsAdmin = targetUser.IsAdmin
        };

        return Ok(dto);
    }



    // GET: api/MyUsers/library/{id}
    [HttpGet("library/{id:int}")]
    public async Task<IActionResult> GetLibrary(int id)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId == null)
        {
            return Unauthorized(new { error = "Utilizador não autenticado" });
        }

        // Vai buscar o utilizador autenticado
        var currentUser = await _context.MyUser
            .FirstOrDefaultAsync(u => u.AspUser == currentUserId);

        if (currentUser == null)
        {
            return NotFound(new { error = "Utilizador autenticado não encontrado" });
        }

        // Verifica se pode aceder à biblioteca do utilizador com o ID pedido
        if (!currentUser.IsAdmin && id != currentUser.UserId)
        {
            return Forbid(); // 403
        }

        // procurar o utilizador alvo
        var user = await _context.MyUser
            .Include(u => u.Purchases)
                .ThenInclude(p => p.Game)
                    .ThenInclude(g => g.Genres)
            .FirstOrDefaultAsync(u => u.UserId == id);

        if (user == null)
        {
            return NotFound(new { error = "Utilizador alvo não encontrado" });
        }

        var library = user.Purchases
            .OrderByDescending(p => p.PurchaseDate)
            .Select(p => new
            {
                purchaseId = p.PurchaseId,
                gameName = p.Game.Title,
                gameDescription = p.Game.Description,
                gamePrice = p.Game.Price,
                purchaseDate = p.PurchaseDate,
                purchasePrice = p.PricePaid,
                genres = p.Game.Genres?.Select(g => g.GenreName).ToList() ?? new List<string>()
            })
            .ToList();

        return Ok(new
        {
            userId = user.UserId,
            library = library,
            totalGames = library.Count
        });
    }
}