using System.ComponentModel.DataAnnotations;

namespace Smart_Gym.Models
{
    public class Factura
    {
        public int Id { get; set; }
        public decimal MontoTotal { get; set; }
        public string? Descripcion { get; set; }
        public DateOnly Fecha { get; set; }

        // Llave Foranea

        public int MembresiaId { get; set; }
        public virtual Membresia Membresia { get; set; } = null!;
    }
}
