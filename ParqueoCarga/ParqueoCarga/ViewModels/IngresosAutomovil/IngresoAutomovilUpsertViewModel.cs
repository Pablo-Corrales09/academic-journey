using System.ComponentModel.DataAnnotations;

namespace ParqueoCarga.ViewModels.IngresosAutomovil;

public sealed class IngresoAutomovilUpsertViewModel
{
    [Range(1u, uint.MaxValue, ErrorMessage = "El ID del parqueo debe ser mayor a 0.")]
    public uint IdParqueo { get; set; }

    [Range(1u, uint.MaxValue, ErrorMessage = "El ID del automóvil debe ser mayor a 0.")]
    public uint IdAutomovil { get; set; }

    [Required(ErrorMessage = "La fecha de entrada es obligatoria.")]
    public DateTime FechaEntrada { get; set; } = DateTime.Now;

    public DateTime? FechaSalida { get; set; }
}
