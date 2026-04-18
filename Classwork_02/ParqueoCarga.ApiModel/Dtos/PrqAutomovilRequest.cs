using System.ComponentModel.DataAnnotations;

namespace ParqueoCarga.ApiModel.Dtos;

/// <summary>Payload for creating or updating a <c>PrqAutomovil</c>.</summary>
/// <param name="Color">Vehicle colour (e.g. "Rojo").</param>
/// <param name="Anio">Manufacturing year (e.g. 2022).</param>
/// <param name="Fabricante">Manufacturer / brand name (e.g. "Toyota").</param>
/// <param name="Tipo">Vehicle type (e.g. "Sedán", "SUV").</param>
public sealed record PrqAutomovilRequest(
    [Required] string Color,
    [Range(1900, 2100)] short Anio,
    [Required] string Fabricante,
    [Required] string Tipo
);
