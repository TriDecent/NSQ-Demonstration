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

    Console.WriteLine("Starting NSQ Consumer Demo...");
    Console.WriteLine($"Channel: {channel}");
    Console.WriteLine($"Topic: {ScenarioMetadata.DEMO_TOPIC}");

    var factory = new NSQFactory();
    var consumer = factory.CreateConsumer(ScenarioMetadata.DEMO_TOPIC, channel, (sender, message) =>
    {
      Console.WriteLine($"[Message Received] From: {sender}, Message: {Encoding.UTF8.GetString(message.Body.ToArray())}");
    });

    Console.WriteLine($"Subscribing to topic '{ScenarioMetadata.DEMO_TOPIC}' on channel '{channel}'...");
    await consumer.ConsumeFromAsync(NSQEndpointExtensions.GetLookupdEndpoint());
    Console.WriteLine("Consumer is now listening for messages. Type 'exit' to stop.");

    string input = "";
    while (input is not "exit")
    {
      input = Console.ReadLine()!;
    }

    consumer.Stop();
    Console.WriteLine("Consumer stopped. Exiting...");
  }
}