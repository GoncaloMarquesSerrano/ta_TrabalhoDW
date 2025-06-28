using System.ComponentModel.DataAnnotations;

/// <summary>
/// ViewModel para exposição de dados do utilizador via API
/// </summary>
public class MyUserDTO
{
    public int UserId { get; set; }

    /// <summary>
    /// Nome do utilizador
    /// </summary>
    [Display(Name = "Nome")]
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Email do utilizador
    /// </summary>
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Data de criação do utilizador
    /// </summary>
    [Display(Name = "Data de Criação")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Atributo que indica se o utilizador é um administrador
    /// </summary>
    [Display(Name = "Administrador")]
    public bool IsAdmin { get; set; }
}