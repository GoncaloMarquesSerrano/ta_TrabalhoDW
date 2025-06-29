using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ludix.Models;

namespace Ludix.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<LudixContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Criar roles se não existirem
            await SeedRolesAsync(roleManager);

            // Criar utilizadores Identity e MyUsers
            await SeedUsersAsync(userManager, context);

            // Criar géneros 
            await SeedGenresAsync(context);

            // Criar developers e jogos
            await SeedDevelopersAndGamesAsync(context);

            // Criar compras e reviews
            await SeedPurchasesAndReviewsAsync(context);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Developer", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedUsersAsync(UserManager<IdentityUser> userManager, LudixContext context)
        {
            // Admin User
            var adminEmail = "admin@ludix.com";
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminIdentityUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminIdentityUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminIdentityUser, "Admin");

                    var adminMyUser = new MyUser
                    {
                        Username = "Admin",
                        Email = adminEmail,
                        Balance = 10000m,
                        CreatedAt = DateTime.Now.AddMonths(-6),
                        AspUser = adminIdentityUser.Id, 
                        IsAdmin = true,
                        RequestedDeveloper = false
                    };

                    context.MyUser.Add(adminMyUser);
                    await context.SaveChangesAsync(); // Guardar depois de criar o admin
                }
            }
            else
            {
                // Verificar se o MyUser correspondente existe
                var existingMyUser = await context.MyUser.FirstOrDefaultAsync(u => u.Email == adminEmail);
                if (existingMyUser == null)
                {
                    var adminMyUser = new MyUser
                    {
                        Username = "Admin",
                        Email = adminEmail,
                        Balance = 10000m,
                        CreatedAt = DateTime.Now.AddMonths(-6),
                        AspUser = existingAdmin.Id,
                        IsAdmin = true,
                        RequestedDeveloper = false
                    };

                    context.MyUser.Add(adminMyUser);
                    await context.SaveChangesAsync();
                }
            }

            // Developer Users
            var developerData = new[]
            {
                new { Email = "cdprojektred.games@email.com", Username = "CdProjektRed", Website = "https://www.cdprojektred.com/en" },
                new { Email = "fromsoftware.studio@email.com", Username = "FromSoftware", Website = "https://www.fromsoftware.jp/ww/" },
                new { Email = "sega.games@email.com", Username = "Sega", Website = "https://www.sega.com/homepage" }
            };

            foreach (var dev in developerData)
            {
                var existingDev = await userManager.FindByEmailAsync(dev.Email);
                if (existingDev == null)
                {
                    var devIdentityUser = new IdentityUser
                    {
                        UserName = dev.Email,
                        Email = dev.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(devIdentityUser, "Dev123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(devIdentityUser, "Developer");
                    }
                }
            }

            // Regular Users
            var userData = new[]
            {
                new { Email = "joao.silva@email.com", Username = "JoãoSilva" },
                new { Email = "maria.santos@email.com", Username = "MariaSantos" },
                new { Email = "pedro.costa@email.com", Username = "PedroCosta" },
                new { Email = "ana.ferreira@email.com", Username = "AnaFerreira" },
                new { Email = "carlos.rodrigues@email.com", Username = "CarlosRodrigues" }
            };

            foreach (var user in userData)
            {
                var existingUser = await userManager.FindByEmailAsync(user.Email);
                if (existingUser == null)
                {
                    var identityUser = new IdentityUser
                    {
                        UserName = user.Email,
                        Email = user.Email,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(identityUser, "User123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(identityUser, "User");

                        var myUser = new MyUser
                        {
                            Username = user.Username,
                            Email = user.Email,
                            Balance = new Random().Next(50, 500),
                            CreatedAt = DateTime.Now.AddDays(-new Random().Next(30, 365)),
                            AspUser = identityUser.Id, 
                            IsAdmin = false,
                            RequestedDeveloper = false
                        };

                        context.MyUser.Add(myUser);
                    }
                }
                else
                {
                    // Verificar se o MyUser correspondente existe
                    var existingMyUser = await context.MyUser.FirstOrDefaultAsync(u => u.Email == user.Email);
                    if (existingMyUser == null)
                    {
                        var myUser = new MyUser
                        {
                            Username = user.Username,
                            Email = user.Email,
                            Balance = new Random().Next(50, 500),
                            CreatedAt = DateTime.Now.AddDays(-new Random().Next(30, 365)),
                            AspUser = existingUser.Id,
                            IsAdmin = false,
                            RequestedDeveloper = false
                        };

                        context.MyUser.Add(myUser);
                    }
                }
            }

            await context.SaveChangesAsync();
        }

        private static async Task SeedGenresAsync(LudixContext context)
        {
            if (!context.Genre.Any())
            {
                var genres = new[]
                {
                    new Genre { GenreName = "Ação" },
                    new Genre { GenreName = "Aventura" },
                    new Genre { GenreName = "RPG" },
                    new Genre { GenreName = "Estratégia" },
                    new Genre { GenreName = "Simulação" },
                    new Genre { GenreName = "Puzzle" },
                    new Genre { GenreName = "Arcade" },
                    new Genre { GenreName = "Indie" }
                };

                context.Genre.AddRange(genres);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedDevelopersAndGamesAsync(LudixContext context)
        {
            if (!context.Developer.Any())
            {
                // Procurar pelo admin
                var adminUser = await context.MyUser.FirstOrDefaultAsync(u => u.IsAdmin);
                if (adminUser == null)
                {
                    throw new InvalidOperationException("Admin não encontrado.O SeedUsersAsync deve correr primeiro.");
                }

                // Procurar os developers Identity users
                var devEmails = new[] { "cdprojektred.games@email.com", "fromsoftware.studio@email.com", "sega.games@email.com" };
                var devUsers = await context.MyUser.Where(u => devEmails.Contains(u.Email)).ToListAsync();

                // Se não existem MyUsers para os developers, criar
                if (devUsers.Count < 3)
                {
                    // Criar os MyUsers em falta para os developers
                    var developerData = new[]
                    {
                        new { Email = "cdprojektred.games@email.com", Username = "CdProjektRed" },
                        new { Email = "fromsoftware.studio@email.com", Username = "FromSoftware" },
                        new { Email = "sega.games@email.com", Username = "Sega" }
                    };

                    foreach (var dev in developerData)
                    {
                        var existingUser = await context.MyUser.FirstOrDefaultAsync(u => u.Email == dev.Email);
                        if (existingUser == null)
                        {
                            // Encontrar o IdentityUser correspondente
                            var identityUser = await context.Users.FirstOrDefaultAsync(u => u.Email == dev.Email);
                            if (identityUser != null)
                            {
                                var myUser = new MyUser
                                {
                                    Username = dev.Username,
                                    Email = dev.Email,
                                    Balance = new Random().Next(100, 1000),
                                    CreatedAt = DateTime.Now.AddMonths(-new Random().Next(3, 12)),
                                    AspUser = identityUser.Id,
                                    IsAdmin = false,
                                    RequestedDeveloper = false
                                };

                                context.MyUser.Add(myUser);
                            }
                        }
                    }
                    await context.SaveChangesAsync();

                    // Recarregar a lista de developers
                    devUsers = await context.MyUser.Where(u => devEmails.Contains(u.Email)).ToListAsync();
                }

                var developers = new List<Developer>();
                var websiteData = new Dictionary<string, string>
                {
                    { "cdprojektred.games@email.com", "https://www.cdprojektred.com/en" },
                    { "fromsoftware.studio@email.com", "https://www.fromsoftware.jp/ww/" },
                    { "sega.games@email.com", "https://www.sega.com/homepage" }
                };

                foreach (var devUser in devUsers)
                {
                    developers.Add(new Developer
                    {
                        Username = devUser.Username,
                        Email = devUser.Email,
                        Balance = new Random().Next(1000, 5000),
                        CreatedAt = devUser.CreatedAt,
                        AspUser = devUser.AspUser,
                        IsAdmin = false,
                        RequestedDeveloper = false,
                        Website = websiteData.GetValueOrDefault(devUser.Email, "https://www.example.com"),
                        ApprovalDate = DateTime.Now.AddMonths(-new Random().Next(1, 6)),
                        ApprovedByUserId = adminUser.UserId
                    });
                }

                context.Developer.AddRange(developers);
                await context.SaveChangesAsync();

                // Criar jogos
                if (developers.Count >= 3)
                {
                    // Procurar pelos géneros
                    var genres = await context.Genre.ToListAsync();
                    var genreDict = genres.ToDictionary(g => g.GenreName, g => g);

                    // Verificar se existem géneros suficientes
                    if (genres.Count < 4)
                    {
                        throw new InvalidOperationException("Géneros insuficientes. O SeedGenresAsync deve correr primeiro.");
                    }
                    var games = new[]
                    {
                        new Game
                        {
                            Title = "The Witcher 3",
                            Price = 29.99m,
                            Description = "Acompanhe as aventuras de Geralt no mais do novo joga da série The Witcher",
                            ReleaseDate = DateTime.Now.AddMonths(-2),
                            Cover = "TW3_Wild_Hunt.png",
                            DeveloperFk = developers[0].UserId,
                            Genres = new List<Genre> { genreDict["RPG"], genreDict["Aventura"] }
                        },
                        new Game
                        {
                            Title = "Dark Souls 3",
                            Price = 15.99m,
                            Description = "Um jogo de conhecido pelo seu grande nível de desafio.",
                            ReleaseDate = DateTime.Now.AddMonths(-1),
                            Cover = "DS3.jpeg",
                            DeveloperFk = developers[1].UserId,
                            Genres = new List<Genre> { genreDict["Ação"], genreDict["RPG"] }
                        },
                        new Game
                        {
                            Title = "Sonic the Hedgehog",
                            Price = 10.99m,
                            Description = "Acompanhe o Sonic numa aventura clássica",
                            ReleaseDate = DateTime.Now.AddDays(-15),
                            Cover = "sonic.jpeg",
                            DeveloperFk = developers[2].UserId,
                            Genres = new List<Genre> { genreDict["Arcade"], genreDict["Aventura"] }
                        },
                        new Game
                        {
                            Title = "CyberPunk 2077",
                            Price = 59.99m,
                            Description = "Entre no fantástico mundo de Cyberpunk 2077 com V",
                            ReleaseDate = DateTime.Now.AddDays(-30),
                            Cover = "C2077.jpeg",
                            DeveloperFk = developers[0].UserId,
                            Genres = new List<Genre> { genreDict["RPG"], genreDict["Ação"] }
                        },
                        new Game
                        {
                            Title = "Elden Ring",
                            Price = 59.99m,
                            Description = "Jogue o jogo do ano de 2022.",
                            ReleaseDate = DateTime.Now.AddDays(-45),
                            Cover = "Er.jpeg",
                            DeveloperFk = developers[1].UserId,
                            Genres = new List<Genre> { genreDict["RPG"], genreDict["Ação"] }
                        }
                    };

                    context.Game.AddRange(games);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static async Task SeedPurchasesAndReviewsAsync(LudixContext context)
        {
            if (!context.Purchase.Any())
            {
                var users = await context.MyUser.Where(u => !u.IsAdmin).Take(5).ToListAsync();
                var games = await context.Game.ToListAsync();

                if (!users.Any() || !games.Any())
                {
                    return; // Não há users ou games para criar purchases
                }

                var random = new Random();

                // Criar compras
                var purchases = new List<Purchase>();
                foreach (var user in users)
                {
                    // Cada utilizador compra 2-4 jogos aleatórios
                    var numPurchases = random.Next(2, Math.Min(5, games.Count + 1));
                    var selectedGames = games.OrderBy(x => random.Next()).Take(numPurchases);

                    foreach (var game in selectedGames)
                    {
                        purchases.Add(new Purchase
                        {
                            PurchaseDate = DateTime.Now.AddDays(-random.Next(1, 60)),
                            PricePaid = game.Price,
                            UserId = user.UserId,
                            GameId = game.GameId
                        });
                    }
                }

                context.Purchase.AddRange(purchases);
                await context.SaveChangesAsync();

                // Criar reviews (alguns utilizadores fazem review dos jogos que compraram)
                var reviews = new List<Review>();
                foreach (var purchase in purchases.Where(p => random.Next(0, 2) == 0)) // 50% de probabilidade
                {
                    reviews.Add(new Review
                    {
                        Rating = random.Next(3, 5), 
                        ReviewText = GetRandomReviewText(random),
                        ReviewDate = purchase.PurchaseDate.AddDays(random.Next(1, 15)),
                        UserId = purchase.UserId,
                        GameId = purchase.GameId
                    });
                }

                if (reviews.Any())
                {
                    context.Review.AddRange(reviews);
                    await context.SaveChangesAsync();
                }
            }
        }

        private static string GetRandomReviewText(Random random)
        {
            var reviews = new[]
            {
                "Excelente jogo! Recomendo vivamente a todos os fãs do género.",
                "Gráficos impressionantes e boa jogabilidade.",
                "História envolvente e bons personagens.",
                "Diversão garantida para horas a fio.",
                "Jogo viciante com mecânicas inovadoras. Surpreendeu-me positivamente.",
                "Boa experiência no geral, apesar de alguns bugs menores.",
                "Trilha sonora fantástica e ambientação perfeita. Adorei!",
                "Desafio na medida certa. Nem muito fácil nem impossível.",
                "Gostei bastante, mas esperava um pouco mais pelo preço pago.",
                "Jogo sólido com boas ideias. Recomendo para quem gosta do estilo."
            };

            return reviews[random.Next(reviews.Length)];
        }
    }
}