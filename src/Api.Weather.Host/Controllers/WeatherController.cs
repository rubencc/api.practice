using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Weather.Host.Resources;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Weather.Application.Commands;
using Weather.Application.Interfaces;
using Weather.Application.Services;

namespace Api.Weather.Host.Controllers;

[Route("api/[controller]")]
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

    [HttpGet]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> GetForecast([FromQuery] ForecastRequest request, CancellationToken cancellationToken)
    {

        var command = new ForecastCommand() { Address = request.Address, Time = request.Time };
        var validationResult = this.validations.TrueForAll(x => x.IsValid(command).Result);
        
        if(!validationResult)
            return BadRequest();

        var locationInfo = await this.geolocationService.GetCoordinates(request.Address).ConfigureAwait(false);
        var forecast = await this.weatherQueryService.GetForecastAsync(locationInfo, cancellationToken).ConfigureAwait(false);
        
        //var response = new ForecastResponse() { Address = forecast.PostalCode, Time = forecast.Time, Temperature = forecast.Temperature, Weather = forecast.Weather };
        return Ok();
    }
}