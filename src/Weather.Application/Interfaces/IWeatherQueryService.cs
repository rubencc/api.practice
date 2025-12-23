using Weather.Application.DTOs;

namespace Weather.Application.Interfaces;

public interface IWeatherQueryService : IDisposable
{
    public Task<ForecastDto> GetForecastAsync((string latitude, string longitude) info, CancellationToken cancellationToken = default);
    public Task<IEnumerable<ForecastDto>> GetHistoricalForecastsAsync(string location, CancellationToken cancellationToken = default);
}