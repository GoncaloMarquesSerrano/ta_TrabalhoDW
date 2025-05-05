using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ludix.Models;

namespace Ludix.Data
{
    public class LudixContext : DbContext
    {
        public LudixContext (DbContextOptions<LudixContext> options)
            : base(options)
        {
        }

        public DbSet<Ludix.Models.Review> Review { get; set; } = default!;
    }
}
