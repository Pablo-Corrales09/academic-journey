namespace ParqueoCarga.ViewModels.Parqueos;

public sealed class ParqueosIndexViewModel
{
    public string Orden { get; set; } = "asc";

    public string? Provincia { get; set; }

    public string? Nombre { get; set; }
}
