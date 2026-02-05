using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Weather.Host.Resources;
using Api.Weather.Host.ExceptionHandlers;
using Asp.Versioning;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Weather.Application.Events;
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
    private readonly ISendMessageService _sendMessageService; 

    public WeatherController(IValidator<ForecastRequest> validator, IGeolocationService geolocationService, IWeatherQueryService weatherQueryService, ForecastService forecastService, ISendMessageService sendMessageService)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _geolocationService = geolocationService ?? throw new ArgumentNullException(nameof(geolocationService));
        _weatherQueryService = weatherQueryService ?? throw new ArgumentNullException(nameof(weatherQueryService));
        _forecastService = forecastService ?? throw new ArgumentNullException(nameof(forecastService));
        _sendMessageService = sendMessageService ?? throw new ArgumentNullException(nameof(sendMessageService));
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

        Location locationInfo = await _geolocationService.GetCoordinates(request.Location).ConfigureAwait(false);
        
        if (locationInfo == null)
            throw new NotFoundException($"Location '{request.Location}' not found. Please verify the address.");
        
        var previousForecasts = await _forecastService.GetForecastAsync(locationInfo, request.Time, cancellationToken).ConfigureAwait(false);

        if (previousForecasts.Any())
        {
            await _sendMessageService.SendMessageAsync(new AuditEvent(), cancellationToken).ConfigureAwait(false);
            
            return Ok(previousForecasts.Select(x => new ForecastResponse() 
            { 
                Location = x.Location.Address, 
                Time = x.Time.ToString(CultureInfo.InvariantCulture), 
                Temperature = x.Temperature.ToString(), 
                Weather = x.WeatherDescription 
            }).ToList());
        }
            
        
        var forecast = await _weatherQueryService.GetForecastAsync(locationInfo, cancellationToken).ConfigureAwait(false);
        await _forecastService.AddForecastAsync(request.Location, request.Time, forecast, cancellationToken).ConfigureAwait(false);
        
        var response = new ForecastResponse() 
        { 
            Location = forecast.Location.Address, 
            Time = forecast.Time.ToString(CultureInfo.InvariantCulture), 
            Temperature = forecast.Temperature.ToString(), 
            Weather = forecast.WeatherDescription 
        };
        
        await _sendMessageService.SendMessageAsync(new AuditEvent(), cancellationToken).ConfigureAwait(false);
        
        return Ok(response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetRecord(string address, CancellationToken cancellationToken)
    {
        var forecast  = await _forecastService.GetRecord(address, cancellationToken).ConfigureAwait(false);
        var response = new ForecastResponse() 
        { 
            Location = forecast.Location.Address, 
            Time = forecast.Time.ToString(CultureInfo.InvariantCulture), 
            Temperature = forecast.Temperature.ToString(), 
            Weather = forecast.WeatherDescription 
        };
        
        await _sendMessageService.SendMessageAsync(new AuditEvent(), cancellationToken).ConfigureAwait(false);
        
        return Ok(response);
    }
}