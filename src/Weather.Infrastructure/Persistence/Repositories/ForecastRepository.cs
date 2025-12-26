using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Weather.Domain.Aggregates;
using Weather.Domain.Repositories;
using Weather.Infrastructure.Configuration;

namespace Weather.Infrastructure.Persistence.Repositories;

public class ForecastRepository : IForecastRepository
{
    private readonly IMongoCollection<Forecast> _forecastCollection;

    public ForecastRepository(IMongoDatabase database, IOptions<MongoDbSettings> settings)
    {
        var mongoSettings = settings.Value;
        _forecastCollection = database.GetCollection<Forecast>(mongoSettings.CollectionName);
    }

    public async Task<bool> AddForecastAsync(Forecast forecast, CancellationToken cancellationToken = default)
    {
        try
        {
            await _forecastCollection.InsertOneAsync(forecast, cancellationToken: cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        // MongoDB client handles connection pooling internally
        // No explicit disposal needed
    }
}

