using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ludix.Models
{
    public class Wishlist
    {
        [Key]
        public int WishlistId { get; set; }

        [Required]
        public int GameId { get; set; }

        [ForeignKey("GameId")]
        public Game Game { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public MyUser User { get; set; }

        public DateTime AddedDate { get; set; } = DateTime.Now;
    }
}