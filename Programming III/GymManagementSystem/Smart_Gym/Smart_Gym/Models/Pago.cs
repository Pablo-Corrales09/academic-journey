namespace Smart_Gym.Models
{
    public class Pago
    {
        public int Id { get; set; }
        public DateOnly FechaPago { get; set; }
        public decimal Monto { get; set; }
        public string CodigoTransaccion { get; set; } = null!;

        public int MembresiaId { get; set; }
        public virtual Membresia Membresia { get; set; } = null!;
    }
}
