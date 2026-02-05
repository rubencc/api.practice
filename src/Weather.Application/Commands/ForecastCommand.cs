namespace Weather.Application.Commands;

public class ForecastCommand
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Location { get; set; }
    public DateTimeOffset Time { get; set; }
}