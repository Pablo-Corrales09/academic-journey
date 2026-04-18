using System.ComponentModel.DataAnnotations;

namespace ParqueoCarga.ViewModels.Automoviles;

public sealed class AutomovilFilterViewModel
{
    [Range(1900, 2100, ErrorMessage = "El año inicial debe estar entre 1900 y 2100.")]
    public short? AnioInicio { get; set; }

    [Range(1900, 2100, ErrorMessage = "El año final debe estar entre 1900 y 2100.")]
    public short? AnioFin { get; set; }

    [RegularExpression("asc|desc", ErrorMessage = "El orden solo puede ser asc o desc.")]
    public string Orden { get; set; } = "desc";

    [RegularExpression("todos|color|fabricante|tipo", ErrorMessage = "El criterio de búsqueda no es válido.")]
    public string CampoBusqueda { get; set; } = "todos";

    [StringLength(80, ErrorMessage = "El término de búsqueda no puede exceder 80 caracteres.")]
    public string? TerminoBusqueda { get; set; }
}