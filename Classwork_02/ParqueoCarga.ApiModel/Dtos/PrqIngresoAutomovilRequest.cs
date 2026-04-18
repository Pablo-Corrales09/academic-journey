using System.ComponentModel.DataAnnotations;

namespace ParqueoCarga.ApiModel.Dtos;

/// <summary>Payload for creating or updating a <c>PrqIngresoAutomovil</c>.</summary>
/// <param name="IdParqueo">Foreign key — ID of the parking lot.</param>
/// <param name="IdAutomovil">Foreign key — ID of the vehicle.</param>
/// <param name="FechaEntrada">Date and time the vehicle entered the parking lot.</param>
/// <param name="FechaSalida">Date and time the vehicle left (null if still parked).</param>
public sealed record PrqIngresoAutomovilRequest(
    [Range(1u, uint.MaxValue)] uint IdParqueo,
    [Range(1u, uint.MaxValue)] uint IdAutomovil,
    [Required] DateTime FechaEntrada,
    DateTime? FechaSalida
);
