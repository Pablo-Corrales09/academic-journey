namespace ParqueoCarga.Configuration;

public sealed class ApiSettings
{
    public const string SectionName = "ApiSettings";

    public string BaseUrl { get; set; } = string.Empty;
}