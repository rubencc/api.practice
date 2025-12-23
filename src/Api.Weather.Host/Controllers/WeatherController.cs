using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Api.Weather.Host.Resources;
using Api.Weather.Host.Services;
using Api.Weather.Host.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Api.Weather.Host.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly List<IValidation<ForecastRequest>> validations;
    private readonly ForecastService forecastService;

    public WeatherController(IEnumerable<IValidation<ForecastRequest>> validations, ForecastService forecastService)
    {
        if (validations == null)
            throw new ArgumentNullException(nameof(validations));
        
        this.validations = validations.ToList();
        
        if (this.validations.Count == 0)
            throw new InvalidOperationException("At least one validation must be registered in the IoC container.");
        
        this.forecastService = forecastService ?? throw new ArgumentNullException(nameof(forecastService));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetForecast([FromQuery] ForecastRequest request, CancellationToken cancellationToken)
    {

        var validationResult = this.validations.TrueForAll(x => x.IsValid(request).Result);
        
        if(!validationResult)
            return BadRequest();
        
        var forecast = await this.forecastService.GetForecast(request.PostalCode, request.Time);
        
        var response = new ForecastResponse() { PostalCode = forecast.PostalCode, Time = forecast.Time, Temperature = forecast.Temperature, Weather = forecast.Weather };
        return Ok(response);
    }
}