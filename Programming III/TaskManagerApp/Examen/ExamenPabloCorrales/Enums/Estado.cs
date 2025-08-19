using System.ComponentModel;

namespace ExamenPabloCorrales.Enums
{
    public enum Estado
    {
        Pendiente,
        [Description("En progreso")]
        EnProgreso,
        Completada,
        Abandonada

    }
}
