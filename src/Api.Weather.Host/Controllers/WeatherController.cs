using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Api.Weather.Host.Resources;
using Api.Weather.Host.ExceptionHandlers;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Weather.Application.Interfaces;
using Weather.Application.Services;
using Weather.Domain.ValueObjects;
using ValidationException = Api.Weather.Host.ExceptionHandlers.ValidationException;

namespace Api.Weather.Host.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly IValidator<ForecastRequest> _validator;
    private readonly IGeolocationService _geolocationService;
    private readonly IWeatherQueryService _weatherQueryService;
    private readonly ForecastService _forecastService;

    public WeatherController(IValidator<ForecastRequest> validator, IGeolocationService geolocationService, IWeatherQueryService weatherQueryService, ForecastService forecastService)
    {
        this._validator = validator ?? throw new ArgumentNullException(nameof(validator));
        this._geolocationService = geolocationService ?? throw new ArgumentNullException(nameof(geolocationService));
        this._weatherQueryService = weatherQueryService ?? throw new ArgumentNullException(nameof(weatherQueryService));
        this._forecastService = forecastService ?? throw new ArgumentNullException(nameof(forecastService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetForecast([FromBody] ForecastRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken).ConfigureAwait(false);
        
        if(!validationResult.IsValid)
            throw new ValidationException("Invalid forecast request. Please check location and time fields.", validationResult.Errors);

        Location locationInfo = await this._geolocationService.GetCoordinates(request.Location).ConfigureAwait(false);
        
        if (locationInfo == null)
            throw new NotFoundException($"Location '{request.Location}' not found. Please verify the address.");
        
        var forecast = await this._weatherQueryService.GetForecastAsync(locationInfo, cancellationToken).ConfigureAwait(false);
        await _forecastService.AddForecastAsync(request.Location, request.Time, forecast, cancellationToken).ConfigureAwait(false);
        
        var response = new ForecastResponse() 
        { 
            Location = forecast.Location.Address, 
            Time = forecast.Time.ToString(CultureInfo.InvariantCulture), 
            Temperature = forecast.Temperature.ToString(), 
            Weather = forecast.WeatherDescription 
        };
        
        return Ok(response);
    }
}