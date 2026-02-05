using Weather.Domain.ValueObjects;

namespace Weather.Domain.Aggregates;

public class Forecast
{
    internal Forecast(Location location, Temperature temperature, string description, DateTime time)
    {
        Location = location;
        Temperature = temperature;
        Description = description;
        Time = time;
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
    }
    
    public Guid Id { get; init; }
    public Location Location { get; init; }
    public DateTime Time { get; init; }
    public Temperature Temperature { get; init; }
    public string Description { get; init; }
    public DateTime CreatedAt { get; init; } 
    
    public static Forecast Create (Location location, DateTime time, Temperature temperature, string description)
    {
        return new Forecast(location, temperature, description, time);
    }
}