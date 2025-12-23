namespace Weather.Domain.Repositories;

public interface IForecastRepository : IDisposable
{
    Task<bool> AddForecastAsync(Aggregates.Forecast forecast, CancellationToken cancellationToken = default);
}