using System.Text;
using Common;
using NSQ.Address;
using NSQ.Factory;

namespace NSQ.Consumer.Scenarios;

public class Scenario : IScenario
{
  public async Task ExecuteAsync()
  {
    var channel = "demo_channel";

    var factory = new NSQFactory();
    var consumer = factory.CreateConsumer(ScenarioMetadata.DEMO_TOPIC, channel, (sender, message) =>
    {
      Console.WriteLine($"Message received from: {sender}, Body: {Encoding.UTF8.GetString(message.Body.ToArray())}");
    });

    Console.WriteLine($"Subscribing to topic '{ScenarioMetadata.DEMO_TOPIC}' on channel '{channel}'");

    await consumer.ConsumeFromAsync(NSQEndpointExtensions.GetLookupdEndpoint());

    string input = "";
    while (input is not "exit")
    {
      input = Console.ReadLine()!;
    }

    consumer.Stop();
  }
}
