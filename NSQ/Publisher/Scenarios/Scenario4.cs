using System.Diagnostics;
using Common;
using NSQ.Address;
using NSQ.Factory;
using NSQ.Models;
using NSQ.Publisher.Scenarios;

namespace Publisher.Scenarios;

public class Scenario4 : IScenario
{
  private const int DEFAULT_MESSAGE_COUNT = 10000;
  private const int DEFAULT_MESSAGE_SIZE_BYTES = 1000;
  private const int DEFAULT_BATCH_SIZE = 100;
  private const string PERFORMANCE_TOPIC = ScenarioMetadata.PERFORMANCE_TESTING_TOPIC;

  public async Task ExecuteAsync()
  {
    Console.WriteLine("Performance Testing Scenario - Publisher");
    Console.WriteLine("========================================");

    Console.Write("Number of messages to send (default 10000): ");
    if (!int.TryParse(Console.ReadLine(), out int messageCount) || messageCount <= 0)
      messageCount = DEFAULT_MESSAGE_COUNT;

    Console.Write("Message size in bytes (default 1000): ");
    if (!int.TryParse(Console.ReadLine(), out int messageSize) || messageSize <= 0)
      messageSize = DEFAULT_MESSAGE_SIZE_BYTES;

    Console.Write("Batch size for publishing (default 100): ");
    if (!int.TryParse(Console.ReadLine(), out int batchSize) || batchSize <= 0)
      batchSize = DEFAULT_BATCH_SIZE;

    Console.WriteLine($"Preparing to send {messageCount} messages of size {messageSize} bytes with batch size {batchSize}");
    Console.WriteLine("Press Enter to start...");
    Console.ReadLine();

    var publisher = new NSQFactory()
      .CreatePublisher(NSQEndpointExtensions.GetDaemonEndpoint(), TimeSpan.FromSeconds(30));

    var payload = GenerateRandomPayload(messageSize);

    var stopwatch = Stopwatch.StartNew();
    int sentMessages = 0;

    for (int i = 0; i < messageCount; i += batchSize)
    {
      var currentBatchSize = Math.Min(batchSize, messageCount - i);
      var messages = new List<Message>(currentBatchSize);

      for (int j = 0; j < currentBatchSize; j++)
      {
        messages.Add(new Message("performance-test-publisher", payload));
      }

      await publisher.MultiPublishAsync(PERFORMANCE_TOPIC, messages);
      sentMessages += currentBatchSize;

      if (sentMessages % 1000 == 0 || sentMessages == messageCount)
      {
        Console.WriteLine($"Sent {sentMessages}/{messageCount} messages...");
      }
    }

    stopwatch.Stop();

    double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
    double messagesPerSecond = messageCount / elapsedSeconds;
    double mbPerSecond = messageCount * messageSize / (1024.0 * 1024.0) / elapsedSeconds;

    Console.WriteLine("\nPerformance Results:");
    Console.WriteLine($"Total time: {elapsedSeconds:F2} seconds");
    Console.WriteLine($"Throughput: {messagesPerSecond:F2} messages/second");
    Console.WriteLine($"           {mbPerSecond:F2} MB/second");

    publisher.Stop();
  }

  private static ReadOnlyMemory<byte> GenerateRandomPayload(int sizeInBytes)
  {
    var random = new Random();
    var buffer = new byte[sizeInBytes];
    random.NextBytes(buffer);
    return buffer;
  }
}