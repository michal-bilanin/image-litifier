using System.Text.Json;
using Azure.Messaging.ServiceBus;
using ErrorOr;

namespace ImageLitifier.WebApp.Services;

public class AzureServiceBus : IAzureServiceBus, IAsyncDisposable
{
    private readonly ServiceBusClient _client;

    public AzureServiceBus()
    {
        var connectionString = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.ServiceBusConnectionString)
            ?? throw new InvalidOperationException("SERVICE_BUS_CONNECTION_STRING environment variable is not set.");

        _client = new ServiceBusClient(connectionString);
    }

    public async Task<ErrorOr<Success>> SendMessageAsync<T>(T message, string queueName)
    {
        var sender = _client.CreateSender(queueName);
        var serviceBusMessage = new ServiceBusMessage(JsonSerializer.Serialize(message));
        
        try
        {
            await sender.SendMessageAsync(serviceBusMessage);
            return new Success();
        }
        catch (Exception ex)
        {
            return Error.Failure(description: $"Failed to send message to Service Bus: {ex.Message}");
        }
        finally
        {
            await sender.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _client.DisposeAsync();
    }
}
