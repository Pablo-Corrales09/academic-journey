using System.ComponentModel.DataAnnotations;

namespace Smart_Gym.Models
{
    public class Gimnasio
    {

        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del gimnasio es obligatorio.")]
        [StringLength(100, MinimumLength = 1, ErrorMessage ="El nombre del gimnasio no puede exceder 100 caracteres.")]
        public string Nombre { get; set; }

        [StringLength(200, MinimumLength = 1, ErrorMessage ="La dirección del gimnasio no puede exceder 200 caracteres.")]
        public string Direccion { get; set; }
        
        public string Telefono { get; set; }

        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();


    }
}
