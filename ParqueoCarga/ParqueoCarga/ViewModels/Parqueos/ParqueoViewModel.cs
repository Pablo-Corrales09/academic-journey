namespace ParqueoCarga.ViewModels.Parqueos;

public sealed class ParqueoViewModel
{
    public uint Id { get; set; }

    public string Provincia { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public decimal PrecioHora { get; set; }
}
