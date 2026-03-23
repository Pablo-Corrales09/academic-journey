// See https://aka.ms/new-console-template for more information

using System;
using System.Linq;
using System.Collections.Generic;

class Program
{
    static void Main(string[] args)
    {
        // Example usage
        var prices = new Dictionary<string, decimal>
        {
            { "SJO-MIA", 5.0m }, // Price per kg for SJO to MIA
            { "MIA-SJO", 4.5m }, // Inverse route for testing CalculoDeRetorno
            { "TGU-MIA", 6.5m },
            { "MIA-MAD", 7.0m }
        };

        decimal weight = 15.5m;
        string departure = "SJO";
        string arrival = "MIA";

        try
        {
            decimal tariff = CalcularTarifaEnvio(weight, departure, arrival, prices, out string log);
            Console.WriteLine($"The shipping rate for {weight} kg from {departure} to {arrival} is: {tariff:C}");
            Console.WriteLine($"Log: {log}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static void GestionZonasDesconocidas(string departureCode, string arrivalCode, IDictionary<string, decimal> prices)
    {
        var availableZones = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var route in prices.Keys)
        {
            var parts = route.Split('-');
            if (parts.Length == 2)
            {
                availableZones.Add(parts[0]);
                availableZones.Add(parts[1]);
            }
        }

        if (!availableZones.Contains(departureCode))
            throw new ArgumentException($"ERROR: NO SE PROCESÓ: La siguiente zona no se encuentra disponible: {departureCode}");

        if (!availableZones.Contains(arrivalCode))
            throw new ArgumentException($"ERROR: NO SE PROCESÓ: La siguiente zona no se encuentra disponible: {arrivalCode}");
    }

    static decimal CalculoDeRetorno(string departureCode, string arrivalCode, IDictionary<string, decimal> prices, out string outLog)
    {
        GestionZonasDesconocidas(departureCode, arrivalCode, prices);

        string inverseRoute = $"{arrivalCode}-{departureCode}";
        if (!prices.TryGetValue(inverseRoute, out decimal inversePrice))
            throw new ArgumentException($"No direct or inverse route available for {departureCode}-{arrivalCode}.");

        decimal surcharge = Math.Round(inversePrice * 0.10m, 2);
        decimal total = Math.Round(inversePrice + surcharge, 2);
        outLog = $"Sin embargo, se encontró una ruta inversa que tiene un recargo de +{surcharge}+ , por lo que el costo calculado es de +{inversePrice}+ 10% = {total}.";

        return total;
    }

    static decimal RutasConTransbordo(string departureCode, string arrivalCode, IDictionary<string, decimal> prices, out string outLog)
    {
        GestionZonasDesconocidas(departureCode, arrivalCode, prices);

        foreach (var kvp in prices)
        {
            var route = kvp.Key.Split('-');
            if (route.Length != 2) continue;

            if (route[0] != departureCode) continue;
            string connection = route[1];
            string secondLeg = $"{connection}-{arrivalCode}";

            if (prices.TryGetValue(secondLeg, out decimal secondPrice))
            {
                decimal firstPrice = kvp.Value;
                decimal total = Math.Round(firstPrice + secondPrice, 2);
                outLog = $"; sin embargo, se encontró una ruta con transbordo que va de {departureCode} a {connection} y luego a {arrivalCode}, por lo que el costo calculado es de {firstPrice} + {secondPrice} = {total}.";
                return total;
            }
        }

        throw new ArgumentException($"No direct, inverse, or transbordo route available for {departureCode}-{arrivalCode}.");
    }

    static decimal CalcularTarifaEnvio(decimal weight, string departureCode, string arrivalCode, Dictionary<string, decimal> prices, out string log)
    {
        log = string.Empty; // Initialize

        if (weight <= 0)
            throw new ArgumentException("Weight must be positive.");

        if (departureCode.Length != 3 || !departureCode.All(char.IsUpper))
            throw new ArgumentException("Departure code must be 3 uppercase letters.");

        if (arrivalCode.Length != 3 || !arrivalCode.All(char.IsUpper))
            throw new ArgumentException("Arrival code must be 3 uppercase letters.");

        GestionZonasDesconocidas(departureCode, arrivalCode, prices);

        string key = $"{departureCode}-{arrivalCode}";
        decimal total;
        string extraLog = string.Empty;

        if (prices.TryGetValue(key, out decimal pricePerKg))
        {
            total = Math.Round(weight * pricePerKg, 2);
        }
        else
        {
            try
            {
                total = CalculoDeRetorno(departureCode, arrivalCode, prices, out extraLog);
            }
            catch (ArgumentException)
            {
                total = RutasConTransbordo(departureCode, arrivalCode, prices, out extraLog);
            }
        }

        log = $"En la fecha {DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")} se procesó un envío de {weight} kg desde {departureCode} hacia {arrivalCode}. Costo total calculado: {total}. {extraLog}";
        return total;
    }
}
