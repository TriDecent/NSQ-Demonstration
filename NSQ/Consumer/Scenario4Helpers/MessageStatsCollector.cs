using System.Collections.Concurrent;
using NSQ.Models;

namespace Consumer.Scenario4Helpers;

public class MessageStatsCollector
{
  public ConcurrentBag<double> ProcessingTimes { get; } = [];
  public ConcurrentDictionary<string, int> MessageCountByConsumer { get; } = new();
  public ConcurrentDictionary<int, int> MessagesBySize { get; } = new();

  public void InitializeConsumer(string consumerName)
  {
    MessageCountByConsumer[consumerName] = 0;
  }

  public void RecordMessage(string consumerName, Message message)
  {
    var sizeBytes = message.Body.Length;
    MessagesBySize.AddOrUpdate(sizeBytes, 1, (_, value) => value + 1);
    MessageCountByConsumer.AddOrUpdate(consumerName, 1, (_, value) => value + 1);
  }

  public void RecordProcessingTime(double processingTimeMs)
  {
    ProcessingTimes.Add(processingTimeMs);
  }
}
