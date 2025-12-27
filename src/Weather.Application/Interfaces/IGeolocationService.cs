using Weather.Domain.ValueObjects;

namespace Weather.Application.Interfaces;

public interface IGeolocationService: IDisposable
{
    public Task<Location> GetCoordinates(string address);
}