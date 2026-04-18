using System.ComponentModel.DataAnnotations;

namespace ParqueoCarga.ViewModels.Automoviles;

public sealed class AutomovilUpsertViewModel
{
    [Required(ErrorMessage = "El color es obligatorio.")]
    [StringLength(60, ErrorMessage = "El color no puede exceder 60 caracteres.")]
    public string Color { get; set; } = string.Empty;

    [Range(1900, 2100, ErrorMessage = "El año debe estar entre 1900 y 2100.")]
    public short Anio { get; set; } = (short)DateTime.UtcNow.Year;

    [Required(ErrorMessage = "El fabricante es obligatorio.")]
    [StringLength(120, ErrorMessage = "El fabricante no puede exceder 120 caracteres.")]
    public string Fabricante { get; set; } = string.Empty;

    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(80, ErrorMessage = "El tipo no puede exceder 80 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
}