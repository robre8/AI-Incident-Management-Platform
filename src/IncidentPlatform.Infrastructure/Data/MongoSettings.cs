namespace IncidentPlatform.Infrastructure.Data;

public class MongoSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string LogsCollectionName { get; set; } = string.Empty;
}
