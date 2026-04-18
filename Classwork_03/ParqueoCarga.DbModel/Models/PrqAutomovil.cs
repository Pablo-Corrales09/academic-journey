namespace ParqueoCarga.DbModel.Models;

public partial class PrqAutomovil
{
    public uint Id { get; set; }

    public string Color { get; set; } = null!;

    public short Anio { get; set; }

    public string Fabricante { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public virtual ICollection<PrqIngresoAutomovil> PrqIngresoAutomoviles { get; set; } = new List<PrqIngresoAutomovil>();
}