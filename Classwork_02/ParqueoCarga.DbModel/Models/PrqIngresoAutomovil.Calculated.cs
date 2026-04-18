using System.ComponentModel.DataAnnotations.Schema;

namespace ParqueoCarga.DbModel.Models;

public partial class PrqIngresoAutomovil
{
    private const decimal DefaultHourlyRate = 1250m;

    [NotMapped]
    public int? DuracionEstadiaMinutos
    {
        get
        {
            if (FechaSalida is null)
            {
                return null;
            }

            return (int)(FechaSalida.Value - FechaEntrada).TotalMinutes;
        }
    }

    [NotMapped]
    public double? DuracionEstadiaHoras
    {
        get
        {
            if (FechaSalida is null)
            {
                return null;
            }

            return (FechaSalida.Value - FechaEntrada).TotalHours;
        }
    }

    [NotMapped]
    public decimal? MontoTotalPagar
    {
        get
        {
            if (FechaSalida is null)
            {
                return null;
            }

            var stayDurationInHours = (decimal)(FechaSalida.Value - FechaEntrada).TotalHours;
            return decimal.Round(stayDurationInHours * DefaultHourlyRate, 2, MidpointRounding.AwayFromZero);
        }
    }
}