using ExamenPabloCorrales.Enums;
using System.ComponentModel.DataAnnotations;

namespace ExamenPabloCorrales.Models
{
    public class MetaPrincipal
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El Titulo es obligatorio.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El nombre del título debe contener entre 5 a 100 caracteres.")]
        [Display(Name = "Título")]
        public string Titulo { get; set; }

        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La Categoria es obligatoria.")]
        public Categoria Categoria { get; set; }

        [Display(Name = "Fecha creación")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)] //formatea la fecha para mostrarla como dd/MM/yyyy
        [DataType(DataType.Date)] //Este atributo hace que el campo se trate como una fecha, sin la parte de las horas.
        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        [Display(Name = "Fecha límite")]
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)] 
        public DateTime? FechaLimite { get; set; }

        [Display(Name = "Prioridad")]
        public Grado Grado { get; set; }

        public Estado Estado { get; set; }

        // Relación con Tarea
        public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
    }
}
