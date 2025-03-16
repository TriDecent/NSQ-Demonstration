using System.Text;
using NSQ.Consumer;
using NSQ.Consumer.Wrapper;
using NsqSharp;

namespace NSQ.Consumer.Scenarios;

public class Scenario1 : IScenerio
{
  public async Task ExecuteAsync()
  {
    var topic = "topic_test";
    var channel = "my_channel";

    var consumerWrapper = new ConsumerWrapper(new NsqSharp.Consumer(topic, channel));
    var nsqConsumer = new NSQConsumer(consumerWrapper);

    nsqConsumer.MessageReceived += (sender, e) =>
    {
      var message = e.Message;
      Console.WriteLine($"Message received from: {message.Sender}, Body: {Encoding.UTF8.GetString(message.Body.ToArray())}");
    };

    Console.WriteLine($"Subscribing to topic '{topic}' on channel '{channel}'");

    await nsqConsumer.ConsumeFromAsync("127.0.0.1:4161");

    string input = "";
    while (input is not "exit")
    {
      input = Console.ReadLine()!;
    }

    nsqConsumer.Stop();
  }
}
