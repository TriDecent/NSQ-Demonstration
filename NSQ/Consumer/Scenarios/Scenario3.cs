using System.Text;
using Common;
using NSQ.Address;
using NSQ.Consumer.Scenarios;
using NSQ.Factory;
using NSQ.Models;

namespace Consumer.Scenarios;

public class Scenario3 : IScenario
{
  public async Task ExecuteAsync()
  {
    var factory = new NSQFactory();
    var channelName = "multi-publisher-consumer";

    var messageHandler = (string sender, Message message) =>
    {
      var body = Encoding.UTF8.GetString(message.Body.ToArray());
      Console.WriteLine($"Received: {body}");
      Console.WriteLine($"From publisher: {message.Sender}");
      Console.WriteLine(new string('-', 50));
    };

    var consumer = factory.CreateConsumer(
      ScenarioMetadata.MULTIPLE_PUBLISHERS_TOPIC,
      channelName,
      messageHandler);

    await consumer.ConsumeFromAsync(NSQEndpointExtensions.GetLookupdEndpoint());

    string input = "";
    while (input is not "exit")
    {
      input = Console.ReadLine()!;
    }

    consumer.Stop();
  }

}
