// See https://aka.ms/new-console-template for more information

using System;

class Program
{
    static void Main(string[] args)
    {
        // Example usage
        var prices = new Dictionary<string, decimal>
        {
            { "SJO-MIA", 5.0m } // Price per kg for SJO to MIA
        };

        decimal weight = 15.5m;
        string departure = "SJO";
        string arrival = "MIA";

        try
        {
            decimal tariff = CalcularTarifaEnvio(weight, departure, arrival, prices);
            Console.WriteLine($"The shipping rate for {weight} kg from {departure} to {arrival} is: {tariff:C}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    static decimal CalcularTarifaEnvio(decimal weight, string departureCode, string arrivalCode, Dictionary<string, decimal> prices)
    {
        if (weight <= 0)
            throw new ArgumentException("Weight must be positive.");

        if (departureCode.Length != 3 || !departureCode.All(char.IsUpper))
            throw new ArgumentException("Departure code must be 3 uppercase letters.");

        if (arrivalCode.Length != 3 || !arrivalCode.All(char.IsUpper))
            throw new ArgumentException("Arrival code must be 3 uppercase letters.");

        string key = $"{departureCode}-{arrivalCode}";
        if (prices.TryGetValue(key, out decimal pricePerKg))
        {
            return weight * pricePerKg;
        }
        else
        {
            throw new ArgumentException($"No price found for route {key}.");
        }
    }
}
