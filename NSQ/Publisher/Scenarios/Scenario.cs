namespace NSQ.Publisher.Scenarios;

using System.Text;
using Common;
using NSQ.Address;
using NSQ.Factory;

public class Scenario : IScenario
{
  public async Task ExecuteAsync()
  {
    var publisher = new NSQFactory()
      .CreatePublisher(NSQEndpointExtensions.GetDaemonEndpoint(), TimeSpan.FromSeconds(5));

    var input = "hello from publisher";

    while (input is not "exit")
    {
      await publisher.PublishAsync(
        ScenarioMetadata.DEMO_TOPIC,
        new Models.Message("Trí", Encoding.UTF8.GetBytes(input!)));

      input = Console.ReadLine();
    }
  }
}
