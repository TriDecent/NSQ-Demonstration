namespace NSQ.Publisher.Scenarios;

using System.Text;
using Common;
using NSQ.Address;
using NSQ.Factory;

public class Scenario : IScenario
{
  public async Task ExecuteAsync()
  {
    Console.WriteLine("Starting NSQ Publisher Demo...");
    Console.WriteLine($"Topic: {ScenarioMetadata.DEMO_TOPIC}");
    Console.WriteLine("Type a message to publish or 'exit' to stop.");

    var publisher = new NSQFactory()
      .CreatePublisher(NSQEndpointExtensions.GetDaemonEndpoint(), TimeSpan.FromSeconds(5));

    string input = "";
    while (input is not "exit")
    {
      Console.Write("Enter message: ");
      input = Console.ReadLine()!;
      if (input is not "exit")
      {
        await publisher.PublishAsync(
          ScenarioMetadata.DEMO_TOPIC,
          new Models.Message("DemoPublisher", Encoding.UTF8.GetBytes(input)));

        Console.WriteLine($"[Message Sent] Topic: {ScenarioMetadata.DEMO_TOPIC}, Message: {input}");
      }
    }

    Console.WriteLine("Publisher stopped. Exiting...");
  }
}