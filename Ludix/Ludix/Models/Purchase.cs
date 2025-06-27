using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ludix.Models
{
    public class Purchase
    {
        [Key]
        public int PurchaseId { get; set; }

        [Display(Name = "Data da Compra")]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Display(Name = "Preço Pago")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePaid { get; set; }

        [Display(Name = "Método de Pagamento")]
        public string PaymentMethod { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = "Processando";

        // Relacionamento com User (usando int para UserId)
        [ForeignKey("User")]
        public int UserId { get; set; }

        [Display(Name = "Utilizador")]
        public MyUser User { get; set; }

        // Relacionamento com Game
        [ForeignKey("Game")]
        public int GameId { get; set; }

        [Display(Name = "Jogo")]
        public Game Game { get; set; }
    }
}