// Copyright (c) Core. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Text.Json;
using Confluent.Kafka;
using RentalManager.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace RentalManager.Infrastructure.Services;

public class KafkaEventBus : IEventBus, IHostedService
{
    private readonly IProducer<Null, string> _producer;
    private readonly IConsumer<Null, string> _consumer;
    private readonly ILogger<KafkaEventBus> _logger;
    private readonly KafkaSettings _settings;
    private readonly Dictionary<string, List<Func<string, Task>>> _handlers = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private Task? _consumerTask;

    public KafkaEventBus(
        IProducer<Null, string> producer,
        IConsumer<Null, string> consumer,
        ILogger<KafkaEventBus> logger,
        IOptions<KafkaSettings> settings)
    {
        _producer = producer;
        _consumer = consumer;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task PublishAsync<T>(T @event, string? topic = null)
        where T : class
    {
        try
        {
            var eventType = typeof(T).Name;
            var topicName = topic ?? eventType.ToLowerInvariant();
            var message = JsonSerializer.Serialize(@event);

            var kafkaMessage = new Message<Null, string>
            {
                Value = message,
                Headers = new Headers
                {
                    { "eventType", System.Text.Encoding.UTF8.GetBytes(eventType) },
                    { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("O")) }
                }
            };

            await _producer.ProduceAsync(topicName, kafkaMessage);
            _logger.LogInformation("Published event {EventType} to topic {Topic}", eventType, topicName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish event {EventType}", typeof(T).Name);
            throw;
        }
    }

    public Task SubscribeAsync<T>(string topic, Func<T, Task> handler)
        where T : class
    {
        var eventType = typeof(T).Name;
        var handlerWrapper = new Func<string, Task>(async message =>
        {
            try
            {
                var @event = JsonSerializer.Deserialize<T>(message);
                if (@event != null)
                {
                    await handler(@event);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle event {EventType}", eventType);
            }
        });

        if (!_handlers.ContainsKey(topic))
        {
            _handlers[topic] = new List<Func<string, Task>>();
        }

        _handlers[topic].Add(handlerWrapper);
        _logger.LogInformation("Subscribed to topic {Topic} for event type {EventType}", topic, eventType);
        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Kafka Event Bus");

        // Subscribe to all registered topics
        var topics = _handlers.Keys.ToList();
        if (topics.Any())
        {
            _consumer.Subscribe(topics);
            _logger.LogInformation("Subscribed to topics: {Topics}", string.Join(", ", topics));
        }

        // Start consumer task
        _consumerTask = Task.Run(ConsumeMessages, _cancellationTokenSource.Token);
        await Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping Kafka Event Bus");

        _cancellationTokenSource.Cancel();
        if (_consumerTask != null)
        {
            await _consumerTask;
        }

        _consumer.Close();
        _producer.Flush();
        _logger.LogInformation("Kafka Event Bus stopped");
    }

    private async Task ConsumeMessages()
    {
        try
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(_cancellationTokenSource.Token);
                    if (consumeResult?.Message?.Value != null)
                    {
                        var topic = consumeResult.Topic;
                        var message = consumeResult.Message.Value;

                        if (_handlers.TryGetValue(topic, out var handlers))
                        {
                            foreach (var handler in handlers)
                            {
                                await handler(message);
                            }
                        }

                        _consumer.Commit(consumeResult);
                    }
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Error consuming message from Kafka");
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fatal error in Kafka consumer");
        }
    }
}

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";

    public string GroupId { get; set; } = "core-api-group";

    public string SecurityProtocol { get; set; } = "PLAINTEXT";

    public string SaslMechanism { get; set; } = string.Empty;

    public string SaslUsername { get; set; } = string.Empty;

    public string SaslPassword { get; set; } = string.Empty;
}
