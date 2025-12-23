﻿using Weather.Application.DTOs;
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

    public Task<bool> AddForecastAsync(ForecastDto dto, CancellationToken cancellationToken = default)
    {
        var forecast = Forecast.Create(dto.Latitude, dto.Time, dto.Temperature, dto.WeatherDescription);
        return repository.AddForecastAsync(forecast, cancellationToken);
    }


}