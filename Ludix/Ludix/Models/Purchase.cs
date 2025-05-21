using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ludix.Models
{
    /// <summary>
    /// Tabela que representa uma compra de um jogo.
    /// </summary>
    public class Purchase
    {
        /// <summary>
        /// Id da compra
        /// </summary>
        [Key]
        public int PurchaseId { get; set; }

        /// <summary>
        /// Data da compra
        /// </summary>
        [Display(Name = "Data da Compra")]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; }

        /// <summary>
        /// Preco pago pela compra
        /// </summary>
        [Display(Name = "Preço pago")]
        [Required(ErrorMessage = "O {0} é de preenchimento obrigatório.")]
        [DataType(DataType.Currency)]
        public decimal PricePaid { get; set; }

        /****************************** 
         * Definição de relacionamentos
         *****************************/

        /// <summary>
        /// FK para referenciar o utilizador que fez a compra
        /// </summary>
        [Display(Name = "Utilizador")]
        [ForeignKey(nameof(MyUser))]
        public int UserId { get; set; } 

        /// <summary>
        /// FK para referenciar o utilizador que fez a compra
        /// </summary>
        [Display(Name = "Utilizador")]
        public MyUser MyUser { get; set; } = new MyUser();

        /// <summary>
        /// FK para referenciar o jogo que foi comprado
        /// </summary>
        [Display(Name = "Jogo")]
        [ForeignKey(nameof(Game))]
        public int GameId { get; set; }

        /// <summary>
        /// FK para referenciar o jogo que foi comprado
        /// </summary>
        [Display(Name = "Jogo")]
        public Game Game { get; set; } = new Game();
    }
}
