using Weather.Application.DTOs;
using Weather.Domain.ValueObjects;

namespace Weather.Application.Interfaces;

public interface IWeatherQueryService : IDisposable
{
    public Task<ForecastDto> GetForecastAsync(Location location, CancellationToken cancellationToken = default);
    public Task<IEnumerable<ForecastDto>> GetHistoricalForecastsAsync(string location, CancellationToken cancellationToken = default);
}