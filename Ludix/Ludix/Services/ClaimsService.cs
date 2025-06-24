using Ludix.Data;
using Ludix.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace Ludix.Services
{
    public class UserClaimsService : IUserClaimsPrincipalFactory<IdentityUser>
    {
        private readonly UserClaimsPrincipalFactory<IdentityUser> _innerFactory;
        private readonly LudixContext _context;

        public UserClaimsService(
            UserManager<IdentityUser> userManager,
            IOptions<IdentityOptions> optionsAccessor,
            LudixContext context)
        {
            _innerFactory = new UserClaimsPrincipalFactory<IdentityUser>(userManager, optionsAccessor);
            _context = context;
        }

        public async Task<ClaimsPrincipal> CreateAsync(IdentityUser user)
        {
            var principal = await _innerFactory.CreateAsync(user);
            var identity = (ClaimsIdentity)principal.Identity!;

            // Buscar o MyUser correspondente
            var myUser = await _context.MyUser
                .FirstOrDefaultAsync(u => u.AspUser == user.Id);

            if (myUser != null)
            {
                // Adicionar claim de admin se for admin
                if (myUser.IsAdmin)
                {
                    identity.AddClaim(new Claim("IsAdmin", "True"));
                }

                // Verificar se é desenvolvedor
                var developer = await _context.Developer
                    .FirstOrDefaultAsync(d => d.AspUser == user.Id);

                if (developer != null)
                {
                    identity.AddClaim(new Claim("IsDeveloper", "True"));
                }
            }

            return principal;
        }
    }
}