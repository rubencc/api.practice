namespace Api.Practice.Services;

using System;
using System.Threading.Tasks;
using Api.Practice.Entities;

public class ForecastService
{
    public Task<Forecast> GetForecast(string postalCode, string time)
    {
        return Task.FromResult(new Forecast() { PostalCode = postalCode, Time = time, Temperature = new Random().Next(0, 40).ToString(), Weather = "Sunny" });
    }

}