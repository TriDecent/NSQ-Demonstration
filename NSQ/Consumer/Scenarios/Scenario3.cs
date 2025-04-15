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

    Console.WriteLine("Starting NSQ Consumer for Multiple Publishers...");
    Console.WriteLine($"Channel: {channelName}");
    Console.WriteLine($"Topic: {ScenarioMetadata.MULTIPLE_PUBLISHERS_TOPIC}");
    Console.WriteLine("Waiting for messages. Type 'exit' to stop.");

    var messageHandler = (string sender, Message message) =>
    {
      var body = Encoding.UTF8.GetString(message.Body.ToArray());
      Console.WriteLine($"[Message Received]");
      Console.WriteLine($"  From Publisher: {message.Sender}");
      Console.WriteLine($"  Body: {body}");
      Console.WriteLine(new string('-', 50));
    };

    var consumer = factory.CreateConsumer(
      ScenarioMetadata.MULTIPLE_PUBLISHERS_TOPIC,
      channelName,
      messageHandler);

    Console.WriteLine($"Subscribing to topic '{ScenarioMetadata.MULTIPLE_PUBLISHERS_TOPIC}' on channel '{channelName}'...");
    await consumer.ConsumeFromAsync(NSQEndpointExtensions.GetLookupdEndpoint());
    Console.WriteLine("Consumer is now listening for messages.");

    string input = "";
    while (input is not "exit")
    {
      input = Console.ReadLine()!;
    }

    consumer.Stop();
    Console.WriteLine("Consumer stopped. Exiting...");
  }
}
