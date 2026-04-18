namespace ParqueoCarga.ViewModels.Automoviles;

public sealed class AutomovilViewModel
{
    public uint Id { get; set; }

    public string Color { get; set; } = string.Empty;

    public short Anio { get; set; }

    public string Fabricante { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;
}