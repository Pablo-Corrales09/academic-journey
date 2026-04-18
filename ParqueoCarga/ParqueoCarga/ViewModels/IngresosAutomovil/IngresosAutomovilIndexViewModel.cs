namespace ParqueoCarga.ViewModels.IngresosAutomovil;

public sealed class IngresosAutomovilIndexViewModel
{
    public IngresoAutomovilFilterViewModel Filtros { get; set; } = new()
    {
        FechaInicio = DateTime.Today.AddDays(-30),
        FechaFin = DateTime.Today,
        Orden = "desc",
        Criterio = "todos"
    };
}
