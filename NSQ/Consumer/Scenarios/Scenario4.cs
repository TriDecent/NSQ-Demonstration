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

    _logger.WriteLine($"Starting {consumerCount} consumers to receive {expectedMessageCount} messages");
    _inputProvider.WaitForUser("Press Enter to start...");

    await _consumerManager.StartConsumersAsync(
      PERFORMANCE_TOPIC,
      CHANNEL,
      consumerCount,
      expectedMessageCount);

    var statisticsReporter = new StatisticsReporter(_logger);
    statisticsReporter.DisplayResults(
      _performanceMeasurer.ElapsedTime,
      _consumerManager.ReceivedMessageCount,
      _statsCollector.MessageCountByConsumer,
      _statsCollector.MessagesBySize,
      _statsCollector.ProcessingTimes);

    _inputProvider.WaitForUser("\nPress Enter to exit...");
    _consumerManager.StopAllConsumers();
  }
}
