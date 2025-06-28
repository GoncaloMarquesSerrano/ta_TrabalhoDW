namespace Ludix.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


/// <summary>
/// Tabela que representa um jogo.
/// </summary>
public class Game
{
    /// <summary>
    /// Id do jogo
    /// </summary>
    [Key]
    public int GameId { get; set; }

    /// <summary>
    /// Nome do jogo
    /// </summary>
    [Display(Name = "Título")]
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [StringLength(50, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Preco do jogo
    /// </summary>
    [Display(Name = "Preço")]
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    /// <summary>
    /// Descricao do jogo
    /// </summary>
    [Display(Name = "Descrição")]
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [StringLength(500, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.", MinimumLength = 10)]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Data de lancamento do jogo
    /// </summary>
    [Display(Name = "Data de Lançamento")]
    [Required(ErrorMessage = "O campo {0} é obrigatório.")]
    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; }

    /// <summary>
    /// Nome da imagem de capa do jogo no disco rigido
    /// </summary>
    [Display(Name = "Capa")]
    public string Cover { get; set; } = string.Empty;

    /***************************
     * Definicao de relacionamentos
    ***************************/

    // Relacionamentos 1-N

    /// <summary>
    /// FK para referenciar o desenvolvedor do jogo
    /// </summary>
    [Display(Name = "Desenvolvedor")]
    [ForeignKey(nameof(Developer))]
    public int DeveloperFk { get; set; }

    /// <summary>
    /// FK para referenciar o desenvolver do jogo
    /// </summary>
    [Display(Name = "Desenvolvedor")]
    public Developer? Developer { get; set; }

    // Relacionamentos M-N

    /// <summary>
    /// Lista dos generos do jogo
    /// </summary>
    [Display(Name = "Géneros")]
    public ICollection<Genre> Genres { get; set; } = new List<Genre>();

    /// <summary>
    /// Lista de utilizadores que compraram o jogo
    /// </summary>
    [Display(Name = "Compras")]
    public ICollection<Purchase> Purchases { get; set; } = new List<Purchase>();

    /// <summary>
    /// Lista das reviews do jogo feitas pelo utilizador
    /// </summary>
    [Display(Name = "Avaliações")]
    public ICollection<Review> Reviews { get; set; } = new List<Review>();

}
