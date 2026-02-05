using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Weather.Domain.Aggregates;
using Weather.Domain.Repositories;
using Weather.Domain.ValueObjects;
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

    public Task<List<Forecast>> GetForecastAsync(Location location, DateTime time, CancellationToken cancellationToken = default)
    {
        DateTime before = time.AddMinutes(-30);
        DateTime after = time.AddMinutes(30);
        var filter = Builders<Forecast>.Filter.Eq(f => f.Location.Address, location.Address) &
                     (Builders<Forecast>.Filter.Gte(f => f.Time, before) &
                      Builders<Forecast>.Filter.Lte(f => f.Time, after));

        return _forecastCollection.Find(filter).ToListAsync(cancellationToken);
    }

    public void Dispose()
    {
        // MongoDB client handles connection pooling internally
        // No explicit disposal needed
    }
}