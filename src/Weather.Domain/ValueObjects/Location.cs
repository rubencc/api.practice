namespace Weather.Domain.ValueObjects;

public class Location : IEquatable<Location>
{
    internal Location(string address, double lat, double lon)
    {
        Address = address;
        Latitude = lat;
        Longitude = lon;
    }
    
    public string Address { get; init; }
    public double? Latitude { get; init;}
    public double? Longitude { get; init;}

    public static Location Create(string address, double lat, double lon)
    {
        return new Location(address, lat, lon);
    }

    public bool Equals(Location? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Address == other.Address && Nullable.Equals(Latitude, other.Latitude) && Nullable.Equals(Longitude, other.Longitude);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Address, Latitude, Longitude);
    }
}