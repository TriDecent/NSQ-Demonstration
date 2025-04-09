using System.Collections.Concurrent;
using System.Diagnostics;
using Common;
using Common.InputProvider;
using Common.Logger;
using NSQ.Address;
using NSQ.Consumer;
using NSQ.Consumer.Scenarios;
using NSQ.Factory;
using NSQ.Models;

namespace Consumer.Scenarios;

public class Scenario4(ILogger logger, IInputProvider inputProvider) : IScenario
{
  private const string PERFORMANCE_TOPIC = ScenarioMetadata.PERFORMANCE_TESTING_TOPIC;
  private const string CHANNEL = "performance-testing-channel";
  private const int INACTIVITY_TIMEOUT_MS = 3000;

  private readonly ILogger _logger = logger;
  private readonly IInputProvider _inputProvider = inputProvider;

  private int _expectedMessageCount;
  private readonly ConcurrentBag<double> _processingTimes = new ConcurrentBag<double>();
  private readonly ConcurrentDictionary<string, int> _messageCountByConsumer = new();
  private readonly ConcurrentDictionary<int, int> _messagesBySize = new();
  private volatile int _receivedMessageCount = 0;
  private volatile bool _testCompleted = false;
  private readonly Stopwatch _stopwatch = new();
  private volatile bool _isFirstMessageReceived = false;
  private readonly Lock _firstMessageLock = new();

  private long _lastMessageReceivedTicks;
  private volatile bool _isStopwatchRunning = false;
  private readonly Lock _stopwatchLock = new();

  public async Task ExecuteAsync()
  {
    _logger.WriteLine("Performance Testing Scenario - Consumer");
    _logger.WriteLine("========================================");

    int consumerCount = _inputProvider.GetInt("Number of consumers", 3);
    _expectedMessageCount = _inputProvider.GetInt("Expected number of messages", 10000);

    _logger.WriteLine($"Starting {consumerCount} consumers to receive {_expectedMessageCount} messages");
    _inputProvider.WaitForUser("Press Enter to start...");

    var factory = new NSQFactory();
    var consumers = new List<INSQConsumer>();

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
      await consumer.ConsumeFromAsync(NSQEndpointExtensions.GetLookupdEndpoint()); // Prevent race condition
    }

    _logger.WriteLine("Consumers started. Waiting for first message...");

    await Task.Run(async () =>
    {
      while (!_testCompleted)
      {
        await Task.Delay(500);
        if (_isFirstMessageReceived)
        {
          CheckInactivityAndManageStopwatch();

          _logger.WriteLine($"Received {_receivedMessageCount}/{_expectedMessageCount} messages... " +
            $"[Timer: {(_isStopwatchRunning ? "Running" : "Paused")}]");
        }
        else
        {
          _logger.WriteLine("Waiting for first message...");
        }

        if (_receivedMessageCount >= _expectedMessageCount)
        {
          lock (_stopwatchLock)
          {
            if (_isStopwatchRunning)
            {
              _stopwatch.Stop();
              _isStopwatchRunning = false;
            }
          }
          _testCompleted = true;
        }
      }
    });

    _logger.WriteLine("\nPerformance Results:");
    _logger.WriteLine($"Total time: {_stopwatch.Elapsed.TotalSeconds:F2} seconds");
    _logger.WriteLine($"Throughput: {_receivedMessageCount / _stopwatch.Elapsed.TotalSeconds:F2} messages/second");

    _logger.WriteLine("\nMessage distribution across consumers:");
    foreach (var kvp in _messageCountByConsumer)
    {
      _logger.WriteLine($"  {kvp.Key}: {kvp.Value} messages ({(double)kvp.Value / _receivedMessageCount:P2})");
    }

    if (!_messagesBySize.IsEmpty)
    {
      _logger.WriteLine("\nMessage size distribution:");
      var groupedSizes = _messagesBySize
        .GroupBy(x => x.Key / 100 * 100)
        .OrderBy(g => g.Key);

      foreach (var group in groupedSizes)
      {
        int count = group.Sum(x => x.Value);
        _logger.WriteLine($"  {group.Key}-{group.Key + 99} bytes: {count} messages");
      }
    }

    if (!_processingTimes.IsEmpty)
    {
      var times = _processingTimes.Order().ToArray();

      _logger.WriteLine("\nProcessing time statistics:");
      _logger.WriteLine("  In milliseconds:");
      _logger.WriteLine($"    Min: {times.First():F6} ms");
      _logger.WriteLine($"    Max: {times.Last():F6} ms");
      _logger.WriteLine($"    Avg: {times.Average():F6} ms");
      _logger.WriteLine($"    P50: {Percentile(times, 50):F6} ms");
      _logger.WriteLine($"    P95: {Percentile(times, 95):F6} ms");
      _logger.WriteLine($"    P99: {Percentile(times, 99):F6} ms");

      _logger.WriteLine("  In microseconds:");
      _logger.WriteLine($"    Min: {times.First() * 1000:F2} μs");
      _logger.WriteLine($"    Max: {times.Last() * 1000:F2} μs");
      _logger.WriteLine($"    Avg: {times.Average() * 1000:F2} μs");
      _logger.WriteLine($"    P50: {Percentile(times, 50) * 1000:F2} μs");
      _logger.WriteLine($"    P95: {Percentile(times, 95) * 1000:F2} μs");
      _logger.WriteLine($"    P99: {Percentile(times, 99) * 1000:F2} μs");
    }

    _inputProvider.WaitForUser("\nPress Enter to exit...");

    foreach (var consumer in consumers)
    {
      consumer.Stop();
    }
  }

  private void CheckInactivityAndManageStopwatch()
  {
    var currentTicks = DateTime.Now.Ticks;
    var lastTicks = Interlocked.Read(ref _lastMessageReceivedTicks);
    var elapsedMs = (currentTicks - lastTicks) / TimeSpan.TicksPerMillisecond;

    if (elapsedMs > INACTIVITY_TIMEOUT_MS)
    {
      lock (_stopwatchLock)
      {
        if (_isStopwatchRunning)
        {
          double stoppedAtSeconds = _stopwatch.Elapsed.TotalSeconds;
          _stopwatch.Stop();
          _isStopwatchRunning = false;
          _logger.WriteLine($"No messages received for {INACTIVITY_TIMEOUT_MS}ms - Pausing timer at {stoppedAtSeconds:F3}s");
        }
      }
    }
  }

  private void HandleMessage(string consumerName, Message message)
  {
    Interlocked.Exchange(ref _lastMessageReceivedTicks, DateTime.Now.Ticks);

    if (!_isFirstMessageReceived)
    {
      lock (_firstMessageLock)
      {
        if (!_isFirstMessageReceived)
        {
          _stopwatch.Start();
          _isStopwatchRunning = true;
          _isFirstMessageReceived = true;
          _logger.WriteLine("First message received! Starting performance measurement at 0.000s");
        }
      }
    }
    else if (!_isStopwatchRunning)
    {
      lock (_stopwatchLock)
      {
        if (!_isStopwatchRunning)
        {
          double resumeAtSeconds = _stopwatch.Elapsed.TotalSeconds;
          _stopwatch.Start();
          _isStopwatchRunning = true;
          _logger.WriteLine($"Messages resumed - Restarting timer at {resumeAtSeconds:F3}s");
        }
      }
    }

    var processingStopwatch = Stopwatch.StartNew();

    Interlocked.Increment(ref _receivedMessageCount);

    var sizeBytes = message.Body.Length;
    _messagesBySize.AddOrUpdate(sizeBytes, 1, (_, value) => value + 1);

    _messageCountByConsumer.AddOrUpdate(consumerName, 1, (_, value) => value + 1);

    processingStopwatch.Stop();
    double processingTimeMs = processingStopwatch.Elapsed.TotalMilliseconds;
    _processingTimes.Add(processingTimeMs);
  }

  private static double Percentile(double[] sortedData, int percentile)
  {
    if (sortedData.Length == 0)
      return 0;

    double n = percentile / 100.0 * (sortedData.Length - 1);
    int k = (int)n;
    double d = n - k;
    return k >= sortedData.Length - 1
      ? sortedData[^1]
      : sortedData[k] + d * (sortedData[k + 1] - sortedData[k]);
  }
}