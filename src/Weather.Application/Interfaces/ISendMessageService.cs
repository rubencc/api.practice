namespace Weather.Application.Interfaces;

public interface ISendMessageService
{
    Task SendMessageAsync<T>(T message, CancellationToken cancellationToken);
}