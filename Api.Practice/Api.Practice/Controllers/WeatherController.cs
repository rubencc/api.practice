namespace Api.Practice.Controllers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Api.Practice.Resources;
using Api.Practice.Services;
using Api.Practice.Validations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    private readonly IValidation<ForecastRequest> postalCodeValidation;
    private readonly ForecastService forecastService;

    public WeatherController(IValidation<ForecastRequest> postalCodeValidation, ForecastService forecastService)
    {
        this.postalCodeValidation = postalCodeValidation ?? throw new ArgumentNullException(nameof(postalCodeValidation));
        this.forecastService = forecastService ?? throw new ArgumentNullException(nameof(forecastService));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetForecast([FromQuery] ForecastRequest request, CancellationToken cancellationToken)
    {

        var validationResult = await this.postalCodeValidation.IsValid(request);
        
        if(!validationResult)
            return BadRequest();
        
        var forecast = await this.forecastService.GetForecast(request.PostalCode, request.Time);
        
        var response = new ForecastResponse() { PostalCode = forecast.PostalCode, Time = forecast.Time, Temperature = forecast.Temperature, Weather = forecast.Weather };
        return Ok(response);
    }
}