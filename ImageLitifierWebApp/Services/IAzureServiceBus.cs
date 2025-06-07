namespace ImageLitifier.WebApp.Services;

using ErrorOr;

public interface IAzureServiceBus
{
    Task<ErrorOr<Success>> SendMessageAsync<T>(T message, string queueName); 
}
