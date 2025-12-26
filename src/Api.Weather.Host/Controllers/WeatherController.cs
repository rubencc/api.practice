using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Weather.Host.Resources;
using Api.Weather.Host.ExceptionHandlers;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Weather.Application.Commands;
using Weather.Application.Interfaces;
using Weather.Application.Services;

namespace Api.Weather.Host.Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly List<IValidation<ForecastCommand>> validations;
    private readonly IGeolocationService geolocationService;
    private readonly IWeatherQueryService weatherQueryService;
    private readonly ForecastService forecastService;

    public WeatherController(IEnumerable<IValidation<ForecastCommand>> validations, IGeolocationService geolocationService, IWeatherQueryService weatherQueryService, ForecastService forecastService)
    {
        if (validations == null)
            throw new ArgumentNullException(nameof(validations));
        
        this.validations = validations.ToList();
        
        if (this.validations.Count == 0)
            throw new InvalidOperationException("At least one validation must be registered in the IoC container.");
        
        this.geolocationService = geolocationService ?? throw new ArgumentNullException(nameof(geolocationService));
        this.weatherQueryService = weatherQueryService ?? throw new ArgumentNullException(nameof(weatherQueryService));
        this.forecastService = forecastService ?? throw new ArgumentNullException(nameof(forecastService));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetForecast([FromBody] ForecastRequest request, CancellationToken cancellationToken)
    {
        // Validar request
        if (request == null)
            throw new ArgumentNullException(nameof(request), "Request cannot be null");

        var command = new ForecastCommand() { Location = request.Location, Time = request.Time };
        var validationResult = this.validations.TrueForAll(x => x.IsValid(command).Result);
        
        if(!validationResult)
            throw new ValidationException("Invalid forecast request. Please check location and time fields.");

        var locationInfo = await this.geolocationService.GetCoordinates(request.Location).ConfigureAwait(false);
        
        if (locationInfo == (null, null) || string.IsNullOrEmpty(locationInfo.Item1))
            throw new NotFoundException($"Location '{request.Location}' not found. Please verify the address.");
        
        var forecast = await this.weatherQueryService.GetForecastAsync(locationInfo, cancellationToken).ConfigureAwait(false);
        await forecastService.AddForecastAsync(request.Location, request.Time, forecast, cancellationToken).ConfigureAwait(false);
        
        var response = new ForecastResponse() 
        { 
            Location = request.Location, 
            Time = forecast.Time.ToString(CultureInfo.InvariantCulture), 
            Temperature = forecast.Temperature.ToString(CultureInfo.InvariantCulture), 
            Weather = forecast.WeatherDescription 
        };
        
        return Ok(response);
    }
}