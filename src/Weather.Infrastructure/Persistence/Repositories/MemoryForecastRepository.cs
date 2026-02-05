using System.Collections.Concurrent;
using Weather.Domain.Aggregates;
using Weather.Domain.Repositories;
using Weather.Domain.ValueObjects;

namespace Weather.Infrastructure.Persistence.Repositories;

public class MemoryForecastRepository : IForecastRepository
{
    ConcurrentDictionary<Guid, Forecast> _forecasts;

    public MemoryForecastRepository()
    {
        _forecasts = new ConcurrentDictionary<Guid, Forecast>();
    }
    
    public void Dispose()
    {
        // TODO release managed resources here
    }

    public Task<bool> AddForecastAsync(Forecast forecast, CancellationToken cancellationToken = default)
    {
        var result = _forecasts.TryAdd(forecast.Id, forecast);
        return Task.FromResult(result);
    }

    public Task<List<Forecast>> GetForecastAsync(Location location, DateTime time, CancellationToken cancellationToken = default)
    {
        DateTime before = time.AddMinutes(-30);
        DateTime after = time.AddMinutes(30);
        
        foreach (var keyValuePair in _forecasts.Where(x => x.Value.Location.Address == location.Address))
        {
            if (keyValuePair.Value.Time >= before && keyValuePair.Value.Time <= after)
            {
                return Task.FromResult(new List<Forecast> { keyValuePair.Value });
            }
        }
        
        return Task.FromResult(new List<Forecast>());
    }
}