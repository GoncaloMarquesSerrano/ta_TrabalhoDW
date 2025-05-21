using System.ComponentModel.DataAnnotations;

namespace Ludix.Models
{
    /// <summary>
    /// Tabela que representa um desenvolvedor de jogos sendo este uma expecializacao de um user
    /// </summary>
    public class Developer
    {
        /// <summary>
        /// Id do desenvolvedor
        /// </summary>
        [Key]
        public int DeveloperId { get; set; }

        /// <summary>
        /// Nome do desenvolvedor
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Website do desenvolvedor
        /// </summary>
        public string? Website { get; set; }

        // Relacionamento 1:N com MyUser (associando com o usuário que é o desenvolvedor)
        public string AspNetUserId { get; set; }  

        public MyUser AspNetUser { get; set; } 
    }
}

