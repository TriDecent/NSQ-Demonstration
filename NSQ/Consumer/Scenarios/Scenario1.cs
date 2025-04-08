using System.Text;
using NSQ.Address;
using NSQ.Consumer.Scenarios;
using NSQ.Factory;
using NSQ.Models;

namespace Consumer.Scenarios;

public class Scenario1 : IScenario
{
  public async Task ExecuteAsync()
  {
    var topic = "scenario1-topic";
    var channel = "same-channel-for-all";
    var factory = new NSQFactory();
    var action = (string consumerName) => (string sender, Message message) =>
      Console.WriteLine($"[{consumerName}] Message received from: {sender}, Body: {Encoding.UTF8.GetString(message.Body.ToArray())}");

    var consumer1 = factory.CreateConsumer(topic, channel, action("consumer1"));
    var consumer2 = factory.CreateConsumer(topic, channel, action("consumer2"));
    var consumer3 = factory.CreateConsumer(topic, channel, action("consumer3"));

    var tasks = new List<Task>{
      consumer1.ConsumeFromAsync(NSQEndpointExtensions.GetLookupdEndpoint()),
      consumer2.ConsumeFromAsync(NSQEndpointExtensions.GetLookupdEndpoint()),
      consumer3.ConsumeFromAsync(NSQEndpointExtensions.GetLookupdEndpoint()),
    };

    await Task.WhenAll(tasks);

    string input = "";
    while (input is not "exit")
    {
      input = Console.ReadLine()!;
    }

    consumer1.Stop();
    consumer2.Stop();
    consumer3.Stop();
  }
}
