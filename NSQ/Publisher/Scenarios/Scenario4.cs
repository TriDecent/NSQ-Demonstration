using System.Diagnostics;
using Common;
using Common.InputProvider;
using Common.Logger;
using Common.PayloadGenerator;
using NSQ.Address;
using NSQ.Factory;
using NSQ.Models;
using NSQ.Publisher.Scenarios;

namespace Publisher.Scenarios;

public class Scenario4(
  IInputProvider inputProvider,
  ILogger logger,
  IPayloadGenerator payloadGenerator) : IScenario
{
  private const int DEFAULT_MESSAGE_COUNT = 10000;
  private const int DEFAULT_MESSAGE_SIZE_BYTES = 1000;
  private const int DEFAULT_BATCH_SIZE = 100;
  private const string PERFORMANCE_TOPIC = ScenarioMetadata.PERFORMANCE_TESTING_TOPIC;

  private readonly IInputProvider _inputProvider = inputProvider;
  private readonly ILogger _logger = logger;
  private readonly IPayloadGenerator _payloadGenerator = payloadGenerator;

  public async Task ExecuteAsync()
  {
    _logger.WriteLine("Performance Testing Scenario - Publisher");
    _logger.WriteLine("========================================");

    int messageCount = _inputProvider.GetInt("Number of messages to send", DEFAULT_MESSAGE_COUNT);
    int messageSize = _inputProvider.GetInt("Message size in bytes", DEFAULT_MESSAGE_SIZE_BYTES);
    int batchSize = _inputProvider.GetInt("Batch size for publishing", DEFAULT_BATCH_SIZE);

    _logger.WriteLine($"Preparing to send {messageCount} messages of size {messageSize} bytes with batch size {batchSize}");
    _inputProvider.WaitForUser("Press Enter to start...");

    var publisher = new NSQFactory().CreatePublisher(NSQEndpointExtensions.GetDaemonEndpoint(), TimeSpan.FromSeconds(30));

    var payload = _payloadGenerator.GeneratePayload(messageSize);

    var stopwatch = Stopwatch.StartNew();
    int sentMessages = 0;

    for (int i = 0; i < messageCount; i += batchSize)
    {
      int currentBatchSize = Math.Min(batchSize, messageCount - i);
      var messages = new List<Message>(currentBatchSize);

      for (int j = 0; j < currentBatchSize; j++)
      {
        messages.Add(new Message("performance-test-publisher", payload));
      }

      await publisher.MultiPublishAsync(PERFORMANCE_TOPIC, messages);
      sentMessages += currentBatchSize;

      if (sentMessages % 1000 == 0 || sentMessages == messageCount)
      {
        _logger.WriteLine($"Sent {sentMessages}/{messageCount} messages...");
      }
    }

    stopwatch.Stop();
    double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
    double messagesPerSecond = messageCount / elapsedSeconds;
    double mbPerSecond = messageCount * messageSize / (1024.0 * 1024.0) / elapsedSeconds;

    _logger.WriteLine("\nPerformance Results:");
    _logger.WriteLine($"Total time: {elapsedSeconds:F2} seconds");
    _logger.WriteLine($"Throughput: {messagesPerSecond:F2} messages/second");
    _logger.WriteLine($"           {mbPerSecond:F2} MB/second");

    publisher.Stop();
  }
}