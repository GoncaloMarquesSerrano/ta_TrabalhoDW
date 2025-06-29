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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure TPT inheritance
            modelBuilder.Entity<MyUser>().ToTable("Users");
            modelBuilder.Entity<Developer>().ToTable("Developers");
        }

        public DbSet<Ludix.Models.Review> Review { get; set; } = default!;

        public DbSet<Ludix.Models.MyUser> MyUser { get; set; } = default!;
        public DbSet<Ludix.Models.Developer> Developer { get; set; } = default!;
        public DbSet<Ludix.Models.Game> Game { get; set; } = default!;
        public DbSet<Ludix.Models.Genre> Genre { get; set; } = default!;
        public DbSet<Ludix.Models.Wishlist> WishlistItems { get; set; } = default!;
        public DbSet<Ludix.Models.Purchase> Purchase { get; set; }
    }
}
