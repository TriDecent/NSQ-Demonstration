using System.Text;
using NSQ.Address;
using NSQ.Factory;
using NSQ.Publisher.Scenarios;

namespace Publisher.Scenarios;

public class Scenario1 : IScenario
{
  public async Task ExecuteAsync()
  {
    var publisher = new NSQFactory()
      .CreatePublisher(NSQEndpointExtensions.GetDaemonEndpoint(), TimeSpan.FromSeconds(5));

    var input = "hello from publisher";

    while (input is not "exit")
    {
      await publisher.PublishAsync(
        "scenario1-topic",
        new NSQ.Models.Message("Trí", Encoding.UTF8.GetBytes(input!)));

      input = Console.ReadLine();
    }
  }
}
