namespace Smart_Gym.Models
{
    public class Membresia
    {
        public int Id { get; set; }
        public string ClienteId { get; set; }
        public string Tipo { get; set; } = null!; // Mensual, Semestral, Anual
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaExpiracion { get; set; }
        public bool EstaPagada { get; set; } = false;

        // Llaves Foraneas
        public virtual Usuario Cliente { get; set; } = null!;
        public virtual Factura? Factura { get; set; }
        public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    }
}
