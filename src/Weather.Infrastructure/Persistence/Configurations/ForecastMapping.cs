using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
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
                map.MapMember(f => f.Time)
                    .SetSerializer(new DateTimeSerializer(DateTimeKind.Utc, BsonType.Document));
                map.MapMember(f => f.CreatedAt)
                    .SetSerializer(new DateTimeSerializer(DateTimeKind.Utc, BsonType.Document));
            });
        }
    }
}