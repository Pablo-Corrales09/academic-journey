using System.Text;
using Microsoft.Extensions.Configuration;

namespace ParqueoCarga.DbModel.Configuration;

internal static class ConnectionStringFactory
{
    public static string? TryCreateFromConfiguration(IConfiguration configuration)
    {
        var section = configuration.GetSection("DatabaseSettings");
        var server = section["Server"];
        var database = section["Database"];
        var user = section["User"];
        var password = section["Password"];

        if (string.IsNullOrWhiteSpace(server) ||
            string.IsNullOrWhiteSpace(database) ||
            string.IsNullOrWhiteSpace(user) ||
            string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var port = section.GetValue<uint?>("Port") ?? 3306;
        var sslMode = section["SslMode"];
        var caCertificatePath = section["CaCertificatePath"];

        var connectionStringBuilder = new StringBuilder();
        connectionStringBuilder.Append("Server=").Append(Escape(server)).Append(';');
        connectionStringBuilder.Append("Port=").Append(port).Append(';');
        connectionStringBuilder.Append("Database=").Append(Escape(database)).Append(';');
        connectionStringBuilder.Append("User ID=").Append(Escape(user)).Append(';');
        connectionStringBuilder.Append("Password=").Append(Escape(password)).Append(';');
        connectionStringBuilder.Append("SslMode=").Append(Escape(string.IsNullOrWhiteSpace(sslMode) ? "Required" : sslMode)).Append(';');

        if (!string.IsNullOrWhiteSpace(caCertificatePath))
        {
            connectionStringBuilder.Append("SslCa=").Append(Escape(caCertificatePath)).Append(';');
        }

        return connectionStringBuilder.ToString();
    }

    private static string Escape(string value)
    {
        return value.Replace(";", "\\;");
    }
}