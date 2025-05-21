using System.Collections.Generic; // Required for List<T>
using System.Linq; // Required for Any()
using Ludix.Models; // Ensure this namespace contains the Developer class
using Ludix.Data; // Ensure this namespace contains ApplicationDbContext

namespace Ludix.Data
{
    public static class SeedData
    {
        // Base de dados de desenvolvedores
        public static void SeedDevelopers(LudixContext context)
        {
            if (context.Developers.Any()) return;

            var developers = new List<Developer>
            {
                new Developer { Name = "CD Projekt Red" },
                new Developer { Name = "EA Sports" },
                new Developer { Name = "Rockstar Games" },
                new Developer { Name = "Santa Monica Studio" },
                new Developer { Name = "Supergiant Games" },
                new Developer { Name = "Nintendo" },
                new Developer { Name = "Ubisoft" },
                new Developer { Name = "Bethesda Game Studios" },
                new Developer { Name = "FromSoftware" },
                new Developer { Name = "BioWare" },
                new Developer { Name = "Insomniac Games" }
            };

            context.Developers.AddRange(developers);
            context.SaveChanges();
        }
    }
}
public class Developer
{
    public int Id { get; set; } // Assuming an Id property exists
    public string Name { get; set; } // Add this property
}