namespace Weather.Application.Interfaces;

public interface IGeolocationService: IDisposable
{
    public Task<(string lat, string logn)> GetCoordinates(string address);
}