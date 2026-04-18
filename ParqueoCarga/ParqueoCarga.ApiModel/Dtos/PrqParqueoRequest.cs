using System.ComponentModel.DataAnnotations;

namespace ParqueoCarga.ApiModel.Dtos;

/// <summary>Payload for creating or updating a <c>PrqParqueo</c>.</summary>
/// <param name="Provincia">Province where the parking lot is located.</param>
/// <param name="Nombre">Parking lot display name.</param>
/// <param name="PrecioHora">Hourly rate charged by this parking lot.</param>
public sealed record PrqParqueoRequest(
    [Required] string Provincia,
    [Required] string Nombre,
    [Range(0, double.MaxValue)] decimal PrecioHora
);
