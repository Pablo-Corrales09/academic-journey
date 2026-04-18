using System.ComponentModel.DataAnnotations;

namespace ParqueoCarga.ViewModels.Parqueos;

public sealed class ParqueoUpsertViewModel
{
    [Required(ErrorMessage = "La provincia es obligatoria.")]
    [StringLength(120, ErrorMessage = "La provincia no puede exceder 120 caracteres.")]
    public string Provincia { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(160, ErrorMessage = "El nombre no puede exceder 160 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(typeof(decimal), "0", "999999999", ErrorMessage = "El precio por hora debe ser mayor o igual a 0.")]
    public decimal PrecioHora { get; set; }
}
