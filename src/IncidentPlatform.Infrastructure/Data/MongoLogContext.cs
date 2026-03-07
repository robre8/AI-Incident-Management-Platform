using IncidentPlatform.Domain.Entities;
using MongoDB.Driver;

namespace IncidentPlatform.Infrastructure.Data;

public class MongoLogContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoSettings _settings;

    public MongoLogContext(MongoSettings settings)
    {
        _settings = settings;
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public IMongoCollection<LogEntry> Logs =>
        _database.GetCollection<LogEntry>(_settings.LogsCollectionName);
}
