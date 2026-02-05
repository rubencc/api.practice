using System.Globalization;
using Weather.Application.DTOs;
using Weather.Domain.Aggregates;
using Weather.Domain.Repositories;
using Weather.Domain.ValueObjects;

namespace Weather.Application.Services;

public class ForecastService
{
    private readonly IForecastRepository repository;

    public ForecastService(IForecastRepository repo)
    {
        repository = repo ?? throw new ArgumentNullException(nameof(repository));
    }

    public Task<bool> AddForecastAsync(string location, DateTime time, ForecastDto dto, CancellationToken cancellationToken = default)
    {
        var forecast = Forecast.Create(dto.Location, time, dto.Temperature, dto.WeatherDescription);
        return repository.AddForecastAsync(forecast, cancellationToken);
    }

    //TODO: Add instrumentation for monitoring
    public async Task<List<ForecastDto>> GetForecastAsync(Location location, DateTime time, CancellationToken cancellationToken = default)
    {
        var list = await repository.GetForecastAsync(location, time, cancellationToken).ConfigureAwait(false);
        return list.Select(f => new ForecastDto
        {
            Location = f.Location,
            Time = f.Time,
            Temperature = f.Temperature,
            WeatherDescription = f.Description,
        }).ToList();
    }
    
    public async Task<ForecastDto> GetRecord(string address, CancellationToken cancellationToken = default)
    {
        var record = await repository.GetRecord(address, cancellationToken).ConfigureAwait(false);
        return new ForecastDto
        {
            Location = record.Location,
            Time = record.Time,
            Temperature = record.Temperature,
            WeatherDescription = record.Description,
        };
    }
}