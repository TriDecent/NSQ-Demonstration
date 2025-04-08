using System.Text;
using NSQ.Consumer.Wrapper;
using NSQ.Factory;

namespace NSQ.Consumer.Scenarios;

public class Scenario : IScenario
{
  public async Task ExecuteAsync()
  {
    var topic = "topic_test";
    var channel = "my_channel";

    var consumerWrapper = new ConsumerWrapper(new NsqSharp.Consumer(topic, channel));
    var factory = new NSQFactory();
    var consumer = factory.CreateConsumer(topic, channel, (sender, message) =>
    {
      Console.WriteLine($"Message received from: {sender}, Body: {Encoding.UTF8.GetString(message.Body.ToArray())}");
    });

    Console.WriteLine($"Subscribing to topic '{topic}' on channel '{channel}'");

    await consumer.ConsumeFromAsync("127.0.0.1:4161");

    string input = "";
    while (input is not "exit")
    {
      input = Console.ReadLine()!;
    }

    consumer.Stop();
  }
}
