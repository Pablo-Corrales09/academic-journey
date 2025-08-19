using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Smart_Gym.Models
{
    public class EjercicioRutina
    {

        [Key]
        public int IdEjercicioRutina { get; set; }

        [Required(ErrorMessage = "El campo debe agregar la cantidad de repeticiones.")]
        [Range(1, 100, ErrorMessage = "La cantidad de repeticiones deben estar entre 1 y 100")]
        public int Repeticiones { get; set; }

        [Required(ErrorMessage = "El campo debe agregar la cantidad de series.")]
        [Range(1, 100, ErrorMessage = "La cantidad de series deben estar entre 1 y 100")]
        public int Series { get; set; }


        //Llave Foranea
        public int IdEjercicio { get; set; }
        public int IdRutina { get; set; }

        //Navegación

        [ForeignKey("IdEjercicio")]
        public Ejercicio Ejercicio { get; set; }

        [ForeignKey("IdRutina")]
        [Display(Name = "Nombre de la rutina")]
        public Rutina Rutina { get; set; }

    }//Fin clase EjercicioRutina

}

