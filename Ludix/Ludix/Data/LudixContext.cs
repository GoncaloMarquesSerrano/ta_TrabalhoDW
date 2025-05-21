using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ludix.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Ludix.Data
{
    public class LudixContext : IdentityDbContext
    {
        public LudixContext (DbContextOptions<LudixContext> options)
            : base(options)
        {
        }

        public DbSet<Ludix.Models.Review> Review { get; set; } = default!;

        public DbSet<Ludix.Models.MyUser> MyUser { get; set; } = default!;

    }
}
