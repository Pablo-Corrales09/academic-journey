using System.ComponentModel.DataAnnotations;

namespace Smart_Gym.Models
{
    public class Maquina
    {

        [Key]
        public int IdMaquina { get; set; }


        [Required(ErrorMessage = "El campo debe agregar un nombre para la máquina.")]
        [StringLength(40, MinimumLength = 1, ErrorMessage = "El nombre de la máquina debe contener entre 1 a 40 caracteres.")]
        public string NombreMaquina { get; set; }


        [Required(ErrorMessage = "El campo debe agregar una descripción para la máquina.")]
        [StringLength(250, MinimumLength = 1, ErrorMessage = "La descripción de la máquina debe contener entre 1 a 250 caracteres.")]
        public string DescripcionMaquina { get; set; }

        public string ImagenMaquina { get; set; }

        public ICollection<Ejercicio> Ejercicios { get; set; }

    } // Fin clase Maquina

}

