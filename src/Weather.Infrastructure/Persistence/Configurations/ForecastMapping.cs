using MongoDB.Bson.Serialization;
using Weather.Domain.Aggregates;

namespace Weather.Infrastructure.Persistence.Configurations;

public class ForecastMapping
{
    public static void InitializeMap()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(Forecast)))
        {
            BsonClassMap.TryRegisterClassMap<Forecast>(map =>
            {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);
                map.SetIgnoreExtraElementsIsInherited(true);
            });
        }
    }
}