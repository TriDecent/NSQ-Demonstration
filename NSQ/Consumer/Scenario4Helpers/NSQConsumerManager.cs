using System.Diagnostics;
using Common.Logger;
using NSQ.Address;
using NSQ.Consumer;
using NSQ.Factory;
using NSQ.Models;

namespace Consumer.Scenario4Helpers;

public class NSQConsumerManager(ILogger logger, PerformanceMeasurer performanceMeasurer, MessageStatsCollector statsCollector)
{
  private readonly ILogger _logger = logger;
  private readonly PerformanceMeasurer _performanceMeasurer = performanceMeasurer;
  private readonly MessageStatsCollector _statsCollector = statsCollector;
  private readonly List<INSQConsumer> _consumers = [];

  private volatile int _receivedMessageCount = 0;
  private volatile bool _testCompleted = false;
  private int _expectedMessageCount;

  public int ReceivedMessageCount => _receivedMessageCount;

  public async Task StartConsumersAsync(
    string topic,
    string channel,
    int consumerCount,
    int expectedMessageCount,
    bool broadcastMode = false)  // Default to load balancing mode
  {
    _expectedMessageCount = broadcastMode
    ? expectedMessageCount * consumerCount
    : expectedMessageCount; ;
    var factory = new NSQFactory();

    for (int i = 0; i < consumerCount; i++)
    {
      string consumerName = $"consumer-{i + 1}";
      _statsCollector.InitializeConsumer(consumerName);

      // In broadcast mode, create a unique channel for each consumer
      string consumerChannel = broadcastMode
        ? $"{channel}-{i + 1}"
        : channel;

      var consumer = factory.CreateConsumer(
        topic,
        consumerChannel,
        (sender, message) => HandleMessage(consumerName, message)
      );

      _consumers.Add(consumer);
      await consumer.ConsumeFromAsync(NSQEndpointExtensions.GetLookupdEndpoint());

      if (broadcastMode)
      {
        _logger.WriteLine($"  Started {consumerName} on dedicated channel '{consumerChannel}'");
      }
    }

    if (!broadcastMode)
    {
      _logger.WriteLine($"All consumers subscribed to shared channel '{channel}'");
    }

    _logger.WriteLine("Consumers started. Waiting for first message...");

    await MonitorConsumersAsync();
  }

  public void StopAllConsumers()
  {
    foreach (var consumer in _consumers)
    {
      consumer.Stop();
    }
  }

  private void HandleMessage(string consumerName, Message message)
  {
    _performanceMeasurer.NotifyMessageReceived();

    var processingStopwatch = Stopwatch.StartNew();

    Interlocked.Increment(ref _receivedMessageCount);

    _statsCollector.RecordMessage(consumerName, message);

    processingStopwatch.Stop();
    _statsCollector.RecordProcessingTime(processingStopwatch.Elapsed.TotalMilliseconds);
  }

  private async Task MonitorConsumersAsync()
  {
    await Task.Run(async () =>
    {
      while (!_testCompleted)
      {
        await Task.Delay(500);

        if (_performanceMeasurer.HasStarted)
        {
          _performanceMeasurer.CheckInactivity();

          var progressPercentage = (double)_receivedMessageCount / _expectedMessageCount * 100;
          _logger.WriteLine($"Received {_receivedMessageCount}/{_expectedMessageCount} messages... " +
            $"({progressPercentage:F1}%) [Timer: {(_performanceMeasurer.IsRunning ? "Running" : "Paused")}]");
        }
        else
        {
          _logger.WriteLine("Waiting for first message...");
        }

        if (_receivedMessageCount >= _expectedMessageCount)
        {
          _performanceMeasurer.Stop();
          _testCompleted = true;
        }
      }
    });
  }
}
