namespace Weather.Infrastructure.Configuration;

public class MongoDbSettings
{
    public static string SECTION_KEY => "MongoDbSettings";
    
    public string ConnectionString { get; set; }
    public string DatabaseName { get; set; }
    public string CollectionName { get; set; }
}
