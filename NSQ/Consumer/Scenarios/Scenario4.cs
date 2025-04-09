using System.Collections.Concurrent;
using System.Diagnostics;
using Common;
using NSQ.Address;
using NSQ.Consumer;
using NSQ.Consumer.Scenarios;
using NSQ.Factory;
using NSQ.Models;

namespace Consumer.Scenarios;

public class Scenario4 : IScenario
{
  private const string PERFORMANCE_TOPIC = ScenarioMetadata.PERFORMANCE_TESTING_TOPIC;
  private const string CHANNEL = "performance-testing-channel";

  private int _expectedMessageCount;
  private readonly ConcurrentBag<double> _processingTimes = [];
  private readonly ConcurrentDictionary<string, int> _messageCountByConsumer = new();
  private readonly ConcurrentDictionary<int, int> _messagesBySize = new();
  private volatile int _receivedMessageCount = 0;
  private volatile bool _testCompleted = false;
  private readonly Stopwatch _stopwatch = new();
  private volatile bool _isFirstMessageReceived = false;

  public async Task ExecuteAsync()
  {
    Console.WriteLine("Performance Testing Scenario - Consumer");
    Console.WriteLine("========================================");

    Console.Write("Number of consumers (default 3): ");
    if (!int.TryParse(Console.ReadLine(), out int consumerCount) || consumerCount <= 0)
      consumerCount = 3;

    Console.Write("Expected number of messages (default 10000): ");
    if (!int.TryParse(Console.ReadLine(), out _expectedMessageCount) || _expectedMessageCount <= 0)
      _expectedMessageCount = 10000;

    Console.WriteLine($"Starting {consumerCount} consumers to receive {_expectedMessageCount} messages");
    Console.WriteLine("Press Enter to start...");
    Console.ReadLine();

    var factory = new NSQFactory();
    var consumers = new List<INSQConsumer>();
    var tasks = new List<Task>();

    for (int i = 0; i < consumerCount; i++)
    {
      string consumerName = $"consumer-{i + 1}";
      _messageCountByConsumer[consumerName] = 0;

      var consumer = factory.CreateConsumer(
        PERFORMANCE_TOPIC,
        CHANNEL,
        (sender, message) => HandleMessage(consumerName, message)
      );

      consumers.Add(consumer);
      tasks.Add(consumer.ConsumeFromAsync(NSQEndpointExtensions.GetLookupdEndpoint()));
    }

    Console.WriteLine("Consumers started. Waiting for first message...");

    await Task.Run(async () =>
    {
      while (!_testCompleted)
      {
        await Task.Delay(1000);

        if (_isFirstMessageReceived)
        {
          Console.WriteLine($"Received {_receivedMessageCount}/{_expectedMessageCount} messages...");
        }
        else
        {
          Console.WriteLine("Waiting for first message...");
        }

        if (_receivedMessageCount >= _expectedMessageCount)
        {
          _stopwatch.Stop();
          _testCompleted = true;
        }
      }
    });

    Console.WriteLine("\nPerformance Results:");
    Console.WriteLine($"Total time: {_stopwatch.Elapsed.TotalSeconds:F2} seconds");
    Console.WriteLine($"Throughput: {_receivedMessageCount / _stopwatch.Elapsed.TotalSeconds:F2} messages/second");

    Console.WriteLine("\nMessage distribution across consumers:");
    foreach (var kvp in _messageCountByConsumer)
    {
      Console.WriteLine($"  {kvp.Key}: {kvp.Value} messages ({(double)kvp.Value / _receivedMessageCount:P2})");
    }

    if (!_messagesBySize.IsEmpty)
    {
      Console.WriteLine("\nMessage size distribution:");
      foreach (var kvp in _messagesBySize.OrderBy(x => x.Key))
      {
        Console.WriteLine($"  {kvp.Key} KB: {kvp.Value} messages");
      }
    }

    if (!_processingTimes.IsEmpty)
    {
      var times = _processingTimes.Order().ToArray();

      Console.WriteLine("\nProcessing time statistics:");

      Console.WriteLine("  In milliseconds:");
      Console.WriteLine($"    Min: {times.First():F6} ms");
      Console.WriteLine($"    Max: {times.Last():F6} ms");
      Console.WriteLine($"    Avg: {times.Average():F6} ms");
      Console.WriteLine($"    P50: {Percentile(times, 50):F6} ms");
      Console.WriteLine($"    P95: {Percentile(times, 95):F6} ms");
      Console.WriteLine($"    P99: {Percentile(times, 99):F6} ms");

      Console.WriteLine("  In microseconds:");
      Console.WriteLine($"    Min: {times.First() * 1000:F2} μs");
      Console.WriteLine($"    Max: {times.Last() * 1000:F2} μs");
      Console.WriteLine($"    Avg: {times.Average() * 1000:F2} μs");
      Console.WriteLine($"    P50: {Percentile(times, 50) * 1000:F2} μs");
      Console.WriteLine($"    P95: {Percentile(times, 95) * 1000:F2} μs");
      Console.WriteLine($"    P99: {Percentile(times, 99) * 1000:F2} μs");
    }

    Console.WriteLine("\nPress Enter to exit...");
    Console.ReadLine();

    foreach (var consumer in consumers)
    {
      consumer.Stop();
    }
  }

  private void HandleMessage(string consumerName, Message message)
  {
    if (!_isFirstMessageReceived)
    {
      lock (this)
      {
        if (!_isFirstMessageReceived)
        {
          _stopwatch.Start();
          _isFirstMessageReceived = true;
          Console.WriteLine("First message received! Starting performance measurement...");
        }
      }
    }

    var processingStopwatch = Stopwatch.StartNew();

    Interlocked.Increment(ref _receivedMessageCount);

    var sizeKB = (message.Body.Length + 1023) / 1024;
    _messagesBySize.AddOrUpdate(sizeKB, 1, (_, value) => value + 1);

    _messageCountByConsumer.AddOrUpdate(consumerName, 1, (_, value) => value + 1);

    processingStopwatch.Stop();

    double processingTimeMs = processingStopwatch.Elapsed.TotalMilliseconds;
    _processingTimes.Add(processingTimeMs);
  }

  private static double Percentile(double[] sortedData, int percentile)
  {
    if (sortedData.Length == 0)
      return 0;

    var n = percentile / (decimal)100.0 * (sortedData.Length - 1);
    var k = (int)n;
    var d = n - k;

    return k >= sortedData.Length - 1
      ? sortedData[^1]
      : sortedData[k] + (double)d * (sortedData[k + 1] - sortedData[k]);
  }
}