namespace ParqueoCarga.DbModel.Models;

public partial class PrqIngresoAutomovil
{
    public uint Consecutivo { get; set; }

    public uint IdParqueo { get; set; }

    public uint IdAutomovil { get; set; }

    public DateTime FechaEntrada { get; set; }

    public DateTime? FechaSalida { get; set; }

    public virtual PrqAutomovil IdAutomovilNavigation { get; set; } = null!;

    public virtual PrqParqueo IdParqueoNavigation { get; set; } = null!;
}