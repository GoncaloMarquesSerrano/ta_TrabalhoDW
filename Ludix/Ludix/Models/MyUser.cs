using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace Ludix.Models
{
    /// <summary>
    /// Tabela que representa um utilizador.
    /// </summary>
    public class MyUser
    {
        /// <summary>
        /// Id do utilizador
        /// </summary>
        [Key]
        public int UserId { get; set; }

        /// <summary>
        /// Nome do utilizador
        /// </summary>
        [Display(Name = "Nome")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Email do utilizador
        /// </summary>
        [Display(Name = "Email")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [EmailAddress(ErrorMessage = "O campo {0} não é um endereço de email válido.")]
        [StringLength(50, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Saldo do utilizador
        /// </summary>
        [Display(Name = "Saldo")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [DataType(DataType.Currency)]
        [Range(0, 10000, ErrorMessage = "O campo {0} deve estar entre {1} e {2}.")]
        public decimal Balance { get; set; }

        /// <summary>
        /// Data de criacao do utilizador
        /// </summary>
        [Display(Name = "Data de Criação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }


        /// <summary>
        /// Nome do utilizador no ASP.NET Identity
        /// </summary>
        [Required]
        public string AspUser { get; set; } = string.Empty;

        /// <summary>
        /// Indica se o utilizador é administrador
        /// </summary>
        [Display(Name = "Administrador")]
        public bool IsAdmin { get; set; } = false;

        /// <summary>
        /// Indica se o utilizador solicitou se tornar desenvolvedor
        /// </summary>
        [Display(Name = "Solicitou ser desenvolvedor")]
        public bool RequestedDeveloper { get; set; } = false;

        /// <summary>
        /// Website proposto para quando se tornar desenvolvedor (se solicitado)
        /// </summary>
        [Display(Name = "Website proposto")]
        public string? ProposedWebsite { get; set; }

        /// <summary>
        /// Data da solicitação para se tornar desenvolvedor
        /// </summary>
        [Display(Name = "Data da solicitação")]
        public DateTime? DeveloperRequestDate { get; set; }

        /**************
         * Relacionamentos
         **************/

        /// <summary>
        /// Lista das compras do utilizador
        /// </summary>
        [Display(Name = "Compras")]
        public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();
    }
}

