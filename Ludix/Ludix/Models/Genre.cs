using System.ComponentModel.DataAnnotations;

namespace Ludix.Models
{
    /// <summary>
    /// Tabela que representa um genero de jogo.
    /// </summary>
    public class Genre
    {
        /// <summary>
        /// Id do genero
        /// </summary>
        [Key]
        public int GenreId { get; set; }

        /// <summary>
        /// Nome do genero
        /// </summary>
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [Display(Name = "Nome do Género")]
        [StringLength(50, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
        public string GenreName { get; set; } = string.Empty;

        /*****************************
         * Definição de relacionamentos
         ***************************/

        // Relacionamentos M-N

        /// <summary>
        /// Lista dos jogos do genero
        /// </summary>
        [Display(Name = "Jogos")]
        public ICollection<Game> Games { get; set; } = [];

    }
}

