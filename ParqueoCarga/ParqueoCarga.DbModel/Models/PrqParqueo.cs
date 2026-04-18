namespace ParqueoCarga.DbModel.Models;

public partial class PrqParqueo
{
    public uint Id { get; set; }

    public string Provincia { get; set; } = null!;

    public string Nombre { get; set; } = null!;

    public decimal PrecioHora { get; set; }

    public virtual ICollection<PrqIngresoAutomovil> PrqIngresoAutomoviles { get; set; } = new List<PrqIngresoAutomovil>();
}