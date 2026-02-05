using Weather.Domain.Aggregates;
using Weather.Domain.ValueObjects;

namespace Weather.Domain.Repositories;

public interface IForecastRepository : IDisposable
{
    Task<bool> AddForecastAsync(Forecast forecast, CancellationToken cancellationToken = default);
    Task<List<Forecast>> GetForecastAsync(Location location, DateTime time, CancellationToken cancellationToken = default);
}