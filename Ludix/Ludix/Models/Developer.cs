namespace Ludix.Models
{
    /// <summary>
    /// Tabela que representa um desenvolvedor de jogos sendo este uma expecializacao de um user
    /// </summary>
    public class Developer : User
    {
        /// <summary>
        /// Website do desenvolvedor
        /// </summary>
        public string Website { get; set; }
    }
}
}
