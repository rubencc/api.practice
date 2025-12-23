namespace Weather.Domain.Aggregates;

public class Forecast
{
    internal Forecast(string address, string temperature, string description, DateOnly time)
    {
        Address = address;
        Temperature = temperature;
        Description = description;
        Time = time;
    }
    
    public string Address { get; init; }
    public DateOnly Time { get; init; }
    public string Temperature { get; init; }
    public string Description { get; init; }
    
    public static Forecast Create (string address, DateOnly time, string temperature, string description)
    {
        return new Forecast(address, temperature, description, time);
    }
}