using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Weather.Infrastructure.Persistence.Configurations;

public static class DefaultGuidMap
{
    public static void InitializeMap()
    {
        BsonSerializer.TryRegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.CSharpLegacy));

    }
}