using System.ComponentModel.DataAnnotations;
namespace Ludix.Models
{
    /// <summary>
    /// Tabela que representa um utilizador.
    /// </summary>
    public class User
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
        [StringLength(25, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 5)]
        public string Username { get; set; }

        /// <summary>
        /// Email do utilizador
        /// </summary>
        [Display(Name = "Email")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [EmailAddress(ErrorMessage = "O campo {0} não é um endereço de email válido.")]
        [StringLength(50, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
        public string Email { get; set; }

        /// <summary>
        /// Senha do utilizador
        /// </summary>
        [Display(Name = "Senha")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        [StringLength(50, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 8)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

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
    }
}

