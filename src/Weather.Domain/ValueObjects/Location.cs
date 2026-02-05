using System.Globalization;

namespace Weather.Domain.ValueObjects;

public record Location(string Address, double? Latitude, double? Longitude) 
{
    
    public string LatitudeValue => Latitude?.ToString(CultureInfo.InvariantCulture) ?? throw new InvalidOperationException("Latitude is not set.");
    public string LongitudeValue => Longitude?.ToString(CultureInfo.InvariantCulture) ?? throw new InvalidOperationException("Longitude is not set.");

    public static Location Create(string address, double lat, double lon)
    {
        return new Location(address, lat, lon);
    }
}