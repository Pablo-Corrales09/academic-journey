using System.ComponentModel.DataAnnotations;

namespace ParqueoCarga.ViewModels.IngresosAutomovil;

public sealed class IngresoAutomovilFilterViewModel
{
    [RegularExpression("todos|tipo|provincia", ErrorMessage = "El criterio seleccionado no es válido.")]
    public string Criterio { get; set; } = "todos";

    [StringLength(120, ErrorMessage = "El término de búsqueda no puede exceder 120 caracteres.")]
    public string? Termino { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    [RegularExpression("asc|desc", ErrorMessage = "El orden debe ser asc o desc.")]
    public string Orden { get; set; } = "desc";
}
