using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ludix.Models
{
    public class Game
    {
        [Key]
        public int GameId { get; set; }

        [Display(Name = "Título")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(50, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 3)]
        public string Title { get; set; } = string.Empty;

        // Propriedade calculada para compatibilidade
        [NotMapped]
        public string Name => Title;

        [Display(Name = "Preço")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(500, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 10)]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Data de Lançamento")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Display(Name = "Capa")]
        public string Cover { get; set; } = string.Empty;

        // Propriedade calculada para URL completa
        [NotMapped]
        public string CoverImageUrl => $"/covers/{Cover}";

        // Relacionamentos
        [ForeignKey(nameof(Developer))]
        public int DeveloperFk { get; set; }
        public Developer? Developer { get; set; }

        public ICollection<Genre> Genres { get; set; } = new List<Genre>();
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<Wishlist> Wishlist { get; set; } = [];
    }
}