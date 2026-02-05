using MassTransit;
using Weather.Application.Interfaces;

namespace Weather.Application.Services;

public class SendMessageService : ISendMessageService
{
    private readonly string _endpointName = "api.weather.audit.consumer";
    
    private readonly ISendEndpointProvider _sendEndpointProvider;
    public SendMessageService(ISendEndpointProvider sendEndpointProvider)
    {
        _sendEndpointProvider = sendEndpointProvider;
    }
    
    public async Task SendMessageAsync<T>(T message, CancellationToken cancellationToken)
    {
        var endpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"exchange:{_endpointName}"))
            .ConfigureAwait(false);
        
        await endpoint.Send(message, cancellationToken).ConfigureAwait(false);
    }
}