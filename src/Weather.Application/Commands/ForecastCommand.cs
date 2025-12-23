namespace Weather.Application.Commands;

public class ForecastCommand
{
    public string Address { get; set; }
    public DateOnly Time { get; set; }
}