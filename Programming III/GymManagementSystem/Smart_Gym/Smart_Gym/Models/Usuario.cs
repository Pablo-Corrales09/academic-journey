using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Gym.Models
{
    public class Usuario : IdentityUser
    {
        
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string? Cedula { get; set; }
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public double? Salario { get; set; }

        //Llave Foranea
        public int? IdGimnasio { get; set; }

        //Navegación
        [ForeignKey("IdGimnasio")]
        public Gimnasio Gimnasio { get; set; }
        public ICollection<UsuarioClase> ClasesUsuario { get; set; } = new List<UsuarioClase>();

        public ICollection<Clase> ClasesEntrenador { get; set; } = new List<Clase>();

        public ICollection<Membresia> Membresias { get; set; } = new List<Membresia>();

    }
}
