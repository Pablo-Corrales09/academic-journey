using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Gym.Models
{
    public class Ejercicio
    {

        [Key]
        public int IdEjercicio { get; set; }


        [Required(ErrorMessage = "El campo debe agregar un nombre para el ejercicio.")]
        [StringLength(40, MinimumLength = 1, ErrorMessage = "El nombre del ejercicio debe contener entre 1 a 40 caracteres.")]
        public string NombreEjercicio { get; set; }



        [Required(ErrorMessage = "El campo debe agregar una descripción para el ejercicio.")]
        [StringLength(250, MinimumLength = 1, ErrorMessage = "La descripción del ejercicio debe contener entre 1 a 250 caracteres.")]
        public string DescripcionEjercicio { get; set; }


        [StringLength(40, MinimumLength = 1, ErrorMessage = "El grupo muscular debe contener entre 1 a 40 caracteres.")]
        public string GrupoMuscular { get; set; }

        public string ImagenEjercicio { get; set; }


        //Llave Foranea

        public int IdMaquina { get; set; }

        //Navegación
        [ForeignKey("IdMaquina")]

        public Maquina Maquina { get; set; }

        public ICollection<EjercicioRutina> EjerciciosRutina { get; set; }

    }// fin clase Ejercicio
}
