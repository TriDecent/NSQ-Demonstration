using Common;
using Common.InputProvider;
using Common.Logger;
using Consumer.Scenario4Helpers;
using NSQ.Consumer.Scenarios;

namespace Consumer.Scenarios;

public class Scenario4 : IScenario
{
  private const string PERFORMANCE_TOPIC = ScenarioMetadata.PERFORMANCE_TESTING_TOPIC;
  private const string CHANNEL = "performance-testing-channel";

  private readonly ILogger _logger;
  private readonly IInputProvider _inputProvider;
  private readonly NSQConsumerManager _consumerManager;
  private readonly MessageStatsCollector _statsCollector;
  private readonly PerformanceMeasurer _performanceMeasurer;

  public Scenario4(ILogger logger, IInputProvider inputProvider)
  {
    _logger = logger;
    _inputProvider = inputProvider;
    _statsCollector = new MessageStatsCollector();
    _performanceMeasurer = new PerformanceMeasurer(logger);
    _consumerManager = new NSQConsumerManager(logger, _performanceMeasurer, _statsCollector);
  }

  public async Task ExecuteAsync()
  {
    _logger.WriteLine("Performance Testing Scenario - Consumer");
    _logger.WriteLine("========================================");

    int consumerCount = _inputProvider.GetInt("Number of consumers", 3);
    int expectedMessageCount = _inputProvider.GetInt("Expected number of messages", 10000);

    _logger.WriteLine("\nDistribution Mode:");
    _logger.WriteLine("  1. Load Balancing (Each message is processed by one consumer)");
    _logger.WriteLine("  2. Broadcast (Each message is processed by all consumers)");
    int modeChoice = _inputProvider.GetInt("Select mode (1 or 2)", 1);
    bool broadcastMode = modeChoice == 2;

    string distributionDescription = broadcastMode
      ? "broadcast (each consumer will receive all messages)"
      : "load balancing (messages will be distributed among consumers)";

    _logger.WriteLine($"\nStarting {consumerCount} consumers in {distributionDescription} mode");
    _logger.WriteLine($"Expecting to receive {expectedMessageCount} messages");
    _inputProvider.WaitForUser("Press Enter to start...");

    await _consumerManager.StartConsumersAsync(
      PERFORMANCE_TOPIC,
      CHANNEL,
      consumerCount,
      expectedMessageCount, broadcastMode);

    var statisticsReporter = new StatisticsReporter(_logger);
    statisticsReporter.DisplayResults(
      _performanceMeasurer.ElapsedTime,
      _consumerManager.ReceivedMessageCount,
      _statsCollector.MessageCountByConsumer,
      _statsCollector.MessagesBySize,
      _statsCollector.ProcessingTimes, broadcastMode, expectedMessageCount);

    _inputProvider.WaitForUser("\nPress Enter to exit...");
    _consumerManager.StopAllConsumers();
  }
}
