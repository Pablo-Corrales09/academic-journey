using System;
using Xunit;
using System.Collections.Generic;

namespace CalcularTarifaEnvio.test
{
    public class CalcularTarifaEnvioTests
    {
        private readonly Dictionary<string, decimal> prices = new Dictionary<string, decimal>
        {
            { "SJO-MIA", 5.0m },
            { "MIA-SJO", 4.5m },
            { "TGU-MIA", 6.5m },
            { "MIA-MAD", 7.0m },
            { "SJO-MAD", 10.0m }
        };

        [Fact]
        public void DirectRoute_CalculatesCorrectly_ForDifferentWeights()
        {
            decimal weight1 = 10m;
            decimal weight2 = 20.25m;

            decimal result1 = Program.CalcularTarifaEnvio(weight1, "SJO", "MIA", prices, out string log1);
            decimal result2 = Program.CalcularTarifaEnvio(weight2, "SJO", "MIA", prices, out string log2);

            Assert.Equal(50.00m, result1);
            Assert.Equal(101.25m, result2);
            Assert.Contains("Costo total calculado:", log1);
            Assert.Contains("Costo total calculado:", log2);
        }

        [Fact]
        public void RutaConTransbordo_UsesConnectionRoute_AndSumsCorrectly()
        {
            decimal result = Program.CalcularTarifaEnvio(1m, "TGU", "MAD", prices, out string log);

            Assert.Equal(13.5m, result);
            Assert.Contains("ruta con transbordo", log);
            Assert.Contains("TGU a MIA y luego a MAD", log);
        }

        [Fact]
        public void OutputLog_HasExpectedDateTimeFormat()
        {
            decimal result = Program.CalcularTarifaEnvio(2m, "SJO", "MIA", prices, out string log);
            string prefix = "En la fecha ";

            Assert.StartsWith(prefix, log);

            string datetimePart = log.Substring(prefix.Length, 19);
            DateTime parsed = DateTime.ParseExact(datetimePart, "dd-MM-yyyy HH:mm:ss", null);

            Assert.Equal(19, datetimePart.Length);
            Assert.True(parsed.Year >= 2020);
        }

        [Fact]
        public void CalculoDeRetorno_Adds10PercentSurcharge_WhenInverseRouteExists()
        {
            // direct route not in dictionary for X to Y, but Y->X exists
            decimal result = Program.CalcularTarifaEnvio(1m, "MIA", "TGU", prices, out string log);

            // Inverse route used: TGU->MIA = 6.5, 10% surcharge => 7.15
            Assert.Equal(7.15m, result);
            Assert.Contains("ruta inversa", log);
            Assert.Contains("recargo", log);
        }

        [Fact]
        public void UnknownZone_ThrowsCustomException_WhenCityCodeDoesNotExist()
        {
            var ex = Assert.Throws<ArgumentException>(() => Program.CalcularTarifaEnvio(1m, "XXX", "MIA", prices, out string log));
            Assert.Contains("ERROR: NO SE PROCESÓ", ex.Message);
        }
    }
}
