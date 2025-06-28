using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ludix.Models
{
    /// <summary>
    /// Tabela que representa um desenvolvedor de jogos sendo este uma expecializacao de um user
    /// </summary>
    public class Developer: MyUser
    {
        /// <summary>
        /// Website do desenvolvedor
        /// </summary>
        public string? Website { get; set; }

        /// <summary>
        /// Data de aprovação como desenvolvedor
        /// </summary>
        [Display(Name = "Data de aprovação")]
        public DateTime ApprovalDate { get; set; }

        /// <summary>
        /// ID do administrador que aprovou
        /// </summary>
        [Display(Name = "Aprovado por")]
        public int ApprovedByUserId { get; set; }

        /// <summary>
        /// Referência ao administrador que aprovou
        /// </summary>
        [ForeignKey("ApprovedByUserId")]
        public virtual MyUser ApprovedBy { get; set; } = null!;

    }
}

