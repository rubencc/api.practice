namespace Weather.Domain.Aggregates;

public class Forecast
{
    internal Forecast(string location, string temperature, string description, DateTimeOffset time)
    {
        Location = location;
        Temperature = temperature;
        Description = description;
        Time = time;
    }
    
    public string Location { get; init; }
    public DateTimeOffset Time { get; init; }
    public string Temperature { get; init; }
    public string Description { get; init; }
    
    public static Forecast Create (string address, DateTimeOffset time, string temperature, string description)
    {
        return new Forecast(address, temperature, description, time);
    }
}