using System.Globalization;
using Weather.Application.DTOs;
using Weather.Domain.Aggregates;
using Weather.Domain.Repositories;

namespace Weather.Application.Services;

public class ForecastService
{
    private readonly IForecastRepository repository;

    public ForecastService(IForecastRepository repository)
    {
        this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<bool> AddForecastAsync(string location, DateTimeOffset time, ForecastDto dto, CancellationToken cancellationToken = default)
    {
        var forecast = Forecast.Create(location, time, dto.Temperature.ToString(CultureInfo.InvariantCulture), dto.WeatherDescription);
        return repository.AddForecastAsync(forecast, cancellationToken);
    }
}