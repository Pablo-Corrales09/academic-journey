namespace ParqueolCarga.RepositoryModel.Dtos;

public sealed class PrqIngresoAutomovilQueryResult
{
    public uint Consecutivo { get; set; }

    public uint IdParqueo { get; set; }

    public uint IdAutomovil { get; set; }

    public DateTime FechaEntrada { get; set; }

    public DateTime? FechaSalida { get; set; }

    public decimal? MontoTotalPagar { get; set; }
}