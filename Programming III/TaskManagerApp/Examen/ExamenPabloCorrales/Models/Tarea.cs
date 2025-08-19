using ExamenPabloCorrales.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExamenPabloCorrales.Models
{
    public class Tarea
    {

        public int Id { get; set; }

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "El nombre de la descripción debe contener entre 5 a 200 caracteres.")]
        [Display(Name = "Descripción")]
        public string Descripcion { get; set; }

        [Display(Name = "Fecha creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)] //formatea la fecha para mostrarla como dd/MM/yyyy
        [DataType(DataType.Date)] //Este atributo hace que el campo se trate como una fecha, sin la parte de las horas.
        public DateTime FechaCreacion { get; set; } = DateTime.Now;


        [Display(Name = "Fecha límite")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)] 
        [DataType(DataType.Date)] 
        public DateTime? FechaLimite { get; set; }

        public Estado Estado { get; set; }

        [Display(Name = "Difcultad")]
        public Grado Grado  { get; set; }

        [Display(Name = "Tiempo estimado (horas)")]
        [Range(1, 100, ErrorMessage = "El tiempo estimado debe estar entre 1 y 100 horas.")]
        public int TiempoEstimado { get; set; }

        // Relación con MetaPrincipal
        [Display(Name = "Meta relacionada")]
        public int MetaPrincipalId { get; set; }
        public MetaPrincipal  MetaPrincipal{ get; set; }

    }
}
