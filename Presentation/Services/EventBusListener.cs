using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Presentation.Models;

namespace Presentation.Services;

public class EventBusListener : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusProcessor _processor;
    private readonly ServiceBusSender _eventBusSender;
    private readonly IServiceScopeFactory _scopeFactory;

    public EventBusListener(ServiceBusClient client, IServiceScopeFactory scopeFactory, ServiceBusSender eventBusSender)
    {
        _client = client;
        _eventBusSender = eventBusSender;
        _scopeFactory = scopeFactory;
        _processor = _client.CreateProcessor("event-bus", new ServiceBusProcessorOptions());
    }

    private static readonly Dictionary<string, TaskCompletionSource<List<Package>>> _pendingRequests = new();

    public ServiceBusSender Sender => _eventBusSender;

    public Task<List<Package>> RegisterRequest(string correlationId)
    {
        var tcs = new TaskCompletionSource<List<Package>>();
        _pendingRequests[correlationId] = tcs;
        return tcs.Task;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageHandler;
        _processor.ProcessErrorAsync += ProcessErrorHandler;
        await _processor.StartProcessingAsync(stoppingToken);
    }

    private async Task ProcessMessageHandler(ProcessMessageEventArgs args)
    {
        using var scope = _scopeFactory.CreateScope();
        var packageService = scope.ServiceProvider.GetRequiredService<EventService>();

        var message = args.Message;
        var body = message.Body.ToString();
        var eventType = message.ApplicationProperties["EventType"].ToString();

        switch (eventType)
        {
            case "PackageResponse":
                if(!string.IsNullOrEmpty(message.CorrelationId) && _pendingRequests.TryGetValue(message.CorrelationId, out var tcs))
                {
                    try
                    {
                        var packages = JsonSerializer.Deserialize<List<Package>>(body);
                        tcs.SetResult(packages ?? []);
                        _pendingRequests.Remove(message.CorrelationId);
                    }
                    catch (Exception ex)
                    {
                        tcs.SetException(ex);
                        _pendingRequests.Remove(message.CorrelationId);
                    }
                }


                break;

            case "AddPackage":

            default:
                throw new ArgumentException($"Unknown event type: {eventType}");
        }
        await args.CompleteMessageAsync(message);
    }

    private Task ProcessErrorHandler(ProcessErrorEventArgs args)
    {
        // Handle the error
        Console.WriteLine($"Error processing message: {args.Exception}");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await _processor.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}
