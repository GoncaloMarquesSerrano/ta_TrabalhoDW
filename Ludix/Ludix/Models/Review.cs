using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ludix.Models
{
    /// <summary>
    /// Tabela que representa uma avaliacao de um jogo.
    /// </summary>
    public class Review
    {
        /// <summary>
        /// Id da review
        /// </summary>
        [Key]
        public int ReviewId { get; set; }

        /// <summary>
        /// Valor da avaliacao do jogo
        /// </summary>
        [Display(Name = "Avaliação")]
        [Range(0, 10, ErrorMessage = "A {0} deve estar entre {1} e {2}") ]
        public int Rating { get; set; }

        /// <summary>
        /// Texto da avaliacao do jogo
        /// </summary>
        [Display(Name = "Texto da Avaliação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(500, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
        public string ReviewText { get; set; } = string.Empty;

        /// <summary>
        /// Data da avaliacao do jogo
        /// </summary>
        [Display(Name = "Data da Avaliação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [DataType(DataType.Date)]
        public DateTime ReviewDate { get; set; }

        /****************************
         * Definição de relacionamentos
         ***************************/

        // Relacionamentos 1-N
        /// <summary>
        /// FK para referenciar o utilizador que fez a avaliacao
        /// </summary>
        [ForeignKey(nameof(MyUser))]
        [Display(Name = "Utilizador")]
        public int UserId { get; set; }
        /// <summary>
        /// FK para referenciar o utilizador que fez a avaliacao
        /// </summary>
        [Display(Name = "Utilizador")]
        [ValidateNever]
        public MyUser? MyUser { get; set; } = new MyUser();

        /// <summary>
        /// FK para referenciar o jogo que foi avaliado
        /// </summary>
        [ForeignKey(nameof(Game))]
        [Display(Name = "Jogo")]
        public int GameId { get; set; }

        /// <summary>
        /// FK para referenciar o jogo que foi avaliado
        /// </summary>
        [Display(Name = "Jogo")]
        [ValidateNever]
        public Game? Game { get; set; } = new Game();
    }
}
