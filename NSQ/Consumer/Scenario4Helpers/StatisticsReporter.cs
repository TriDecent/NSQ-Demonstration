using System.Collections.Concurrent;
using Common.Logger;

namespace Consumer.Scenario4Helpers;

public class StatisticsReporter(ILogger logger)
{
  private readonly ILogger _logger = logger;

  public void DisplayResults(
    TimeSpan elapsedTime,
    int receivedMessageCount,
    ConcurrentDictionary<string, int> messageCountByConsumer,
    ConcurrentDictionary<int, int> messagesBySize,
    ConcurrentBag<double> processingTimes,
    bool broadcastMode = false, int originalExpectedCount = 0, ConcurrentBag<DateTime>? messageTimestamps = null)
  {
    _logger.WriteLine("\nPerformance Results:");
    _logger.WriteLine($"Mode: {(broadcastMode ? "Broadcast" : "Load Balancing")}");
    _logger.WriteLine($"Total time: {elapsedTime.TotalSeconds:F2} seconds");

    int expectedMessagesPerConsumer = originalExpectedCount;

    double messagesPerSecond = receivedMessageCount / elapsedTime.TotalSeconds;
    double perConsumerThroughput = messagesPerSecond / messageCountByConsumer.Count;

    if (broadcastMode)
    {
      _logger.WriteLine($"Individual throughput: {perConsumerThroughput:F2} messages/second/consumer");
      _logger.WriteLine($"Combined throughput: {messagesPerSecond:F2} messages/second");
    }
    else
    {
      _logger.WriteLine($"Throughput: {messagesPerSecond:F2} messages/second");
    }

    if (messageTimestamps != null && !messageTimestamps.IsEmpty)
    {
      DisplayPerformanceOverTime(messageTimestamps);
    }
    else
    {
      DisplayPerformanceOverTimeFrom(processingTimes);
    }

    DisplayMessageDistribution(
      messageCountByConsumer,
      receivedMessageCount,
      expectedMessagesPerConsumer,
      broadcastMode);

    if (!messagesBySize.IsEmpty)
    {
      DisplayMessageSizeDistribution(messagesBySize);
    }

    if (!processingTimes.IsEmpty)
    {
      DisplayProcessingTimeStatistics(processingTimes, messagesBySize, receivedMessageCount);
    }
  }

  private void DisplayPerformanceOverTime(ConcurrentBag<DateTime> timestamps)
  {
    if (timestamps.IsEmpty)
      return;

    const int intervalSeconds = 5;
    var intervals = new Dictionary<int, int>();

    DateTime startTime = timestamps.Min();

    foreach (var timestamp in timestamps)
    {
      int intervalIndex = (int)((timestamp - startTime).TotalSeconds / intervalSeconds);

      if (!intervals.ContainsKey(intervalIndex))
      {
        intervals[intervalIndex] = 0;
      }
      intervals[intervalIndex]++;
    }

    _logger.WriteLine("\nPerformance over time:");
    foreach (var kvp in intervals.OrderBy(kvp => kvp.Key))
    {
      double startSeconds = kvp.Key * intervalSeconds;
      double endSeconds = (kvp.Key + 1) * intervalSeconds;
      _logger.WriteLine($"  {startSeconds:F1}-{endSeconds:F1} seconds: {kvp.Value} messages");
    }
  }

  private void DisplayMessageDistribution(
    ConcurrentDictionary<string, int> messageCountByConsumer,
    int totalCount,
    int expectedMessagesPerConsumer = 0,
    bool broadcastMode = false)
  {
    _logger.WriteLine("\nMessage distribution across consumers:");

    foreach (var kvp in messageCountByConsumer)
    {
      double percentage = (double)kvp.Value / totalCount;

      if (broadcastMode)
      {
        double completionRate = (double)kvp.Value / expectedMessagesPerConsumer;
        _logger.WriteLine($"  {kvp.Key}: {kvp.Value}/{expectedMessagesPerConsumer} messages ({completionRate:P2} of expected)");
      }
      else
      {
        _logger.WriteLine($"  {kvp.Key}: {kvp.Value} messages ({percentage:P2} of total)");
      }
    }

    if (!broadcastMode && messageCountByConsumer.Count > 1)
    {
      var counts = messageCountByConsumer.Select(kvp => kvp.Value).ToArray();
      int min = counts.Min();
      int max = counts.Max();
      double avg = counts.Average();
      double fairnessIndex = min / avg;

      _logger.WriteLine("\nLoad balancing fairness metrics:");
      _logger.WriteLine($"  Min: {min}, Max: {max}, Avg: {avg:F2}");
      _logger.WriteLine($"  Max/Min ratio: {(double)max / min:F2}");
      _logger.WriteLine($"  Fairness index: {fairnessIndex:F4} (1.0 is perfectly balanced)");
    }
  }

  private void DisplayMessageSizeDistribution(ConcurrentDictionary<int, int> messagesBySize)
  {
    _logger.WriteLine("\nMessage size distribution:");
    var groupedSizes = messagesBySize
      .GroupBy(x => x.Key / 100 * 100)
      .OrderBy(g => g.Key);

    foreach (var group in groupedSizes)
    {
      int count = group.Sum(x => x.Value);
      _logger.WriteLine($"  {group.Key}-{group.Key + 99} bytes: {count} messages");
    }
  }

  private void DisplayProcessingTimeStatistics(
    ConcurrentBag<double> processingTimes,
    ConcurrentDictionary<int, int> messagesBySize,
    int receivedMessageCount)
  {
    var times = processingTimes.Order().ToArray();
    double avg = times.Average();
    double stdDev = Math.Sqrt(times.Average(v => Math.Pow(v - avg, 2)));

    _logger.WriteLine("\nProcessing time statistics:");
    _logger.WriteLine($"  Standard Deviation: {stdDev:F6} ms");

    DisplayProcessingTimeBuckets(times);
    DisplayMessageSizeBuckets(messagesBySize);
    // DisplayPerformanceOverTimeFrom(processingTimes);
    DisplayAggregatePerformance(receivedMessageCount, messagesBySize);
    DisplayDetailedTimeStats(times);
  }

  private void DisplayProcessingTimeBuckets(double[] times)
  {
    var buckets = new[] { 1, 2, 5, 10, 20, 50, 100 };
    var bucketCounts = new int[buckets.Length + 1];

    foreach (var time in times)
    {
      bool bucketed = false;
      for (int i = 0; i < buckets.Length; i++)
      {
        if (time <= buckets[i])
        {
          bucketCounts[i]++;
          bucketed = true;
          break;
        }
      }
      if (!bucketed)
      {
        bucketCounts[^1]++;
      }
    }

    _logger.WriteLine("\nProcessing time distribution:");
    for (int i = 0; i < buckets.Length; i++)
    {
      _logger.WriteLine($"  <= {buckets[i]} ms: {bucketCounts[i]} messages");
    }
    _logger.WriteLine($"  > {buckets[^1]} ms: {bucketCounts[^1]} messages");
  }

  private void DisplayMessageSizeBuckets(ConcurrentDictionary<int, int> messagesBySize)
  {
    var sizes = messagesBySize.Select(kvp => new { Size = kvp.Key, Count = kvp.Value }).ToArray();
    double avgSize = sizes.Sum(x => (long)x.Size * x.Count) / sizes.Sum(x => x.Count);

    _logger.WriteLine("\nMessage size statistics:");
    _logger.WriteLine($"  Average size: {avgSize:F2} bytes");

    var sizeBuckets = new[] { 100, 500, 1000, 2000, 5000 };
    var sizeBucketCounts = new int[sizeBuckets.Length + 1];

    foreach (var size in sizes)
    {
      bool bucketed = false;
      for (int i = 0; i < sizeBuckets.Length; i++)
      {
        if (size.Size <= sizeBuckets[i])
        {
          sizeBucketCounts[i] += size.Count;
          bucketed = true;
          break;
        }
      }
      if (!bucketed)
      {
        sizeBucketCounts[^1] += size.Count;
      }
    }

    _logger.WriteLine("\nMessage size distribution:");
    for (int i = 0; i < sizeBuckets.Length; i++)
    {
      _logger.WriteLine($"  <= {sizeBuckets[i]} bytes: {sizeBucketCounts[i]} messages");
    }
    _logger.WriteLine($"  > {sizeBuckets[^1]} bytes: {sizeBucketCounts[^1]} messages");
  }

  private void DisplayPerformanceOverTimeFrom(ConcurrentBag<double> processingTimes)
  {
    const int intervalSeconds = 10;
    var intervals = new Dictionary<int, int>();

    foreach (var time in processingTimes)
    {
      int interval = (int)(time / intervalSeconds);
      if (!intervals.TryGetValue(interval, out int value))
      {
        value = 0;
        intervals[interval] = value;
      }
      intervals[interval] = ++value;
    }

    _logger.WriteLine("\nPerformance over time:");
    foreach (var kvp in intervals.OrderBy(kvp => kvp.Key))
    {
      _logger.WriteLine($"  {kvp.Key * intervalSeconds}-{(kvp.Key + 1) * intervalSeconds} seconds: {kvp.Value} messages");
    }
  }

  private void DisplayAggregatePerformance(
    int receivedMessageCount,
    ConcurrentDictionary<int, int> messagesBySize)
  {
    var sizes = messagesBySize.Select(kvp => new { Size = kvp.Key, Count = kvp.Value }).ToArray();
    double avgSize = sizes.Sum(x => (long)x.Size * x.Count) / receivedMessageCount;
    double totalDataMB = receivedMessageCount * avgSize / (1024.0 * 1024.0);

    _logger.WriteLine("\nAggregate Performance:");
    _logger.WriteLine($"  Total messages processed: {receivedMessageCount}");
    _logger.WriteLine($"  Total data processed: {totalDataMB:F2} MB");
  }

  private void DisplayDetailedTimeStats(double[] times)
  {
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