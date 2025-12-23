using Weather.Domain.Aggregates;
using Weather.Domain.Repositories;

namespace Weather.Infrastructure.Persistence.Repositories;

public class ForecastRepository : IForecastRepository
{
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<bool> AddForecastAsync(Forecast forecast, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}