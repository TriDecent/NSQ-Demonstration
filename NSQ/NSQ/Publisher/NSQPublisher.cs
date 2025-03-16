namespace NSQ.Publisher;

using System.Text;
using System.Text.Json;
using NSQ.Models;
using NsqSharp.Bus.Configuration;

public class NSQPublisher(INsqdPublisher publisher) : INSQPublisher
{
  private readonly INsqdPublisher _publisher = publisher;

  public Task PublishAsync(string topic, Message message)
  {
    var jsonMessage = JsonSerializer.Serialize(message);
    var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);

    return Task.Run(() => _publisher.Publish(topic, messageBytes));
  }

  public Task MultiPublishAsync(string topic, IEnumerable<Message> messages)
  {
    var messagesBytes = messages.Select(message =>
    {
      var jsonMessage = JsonSerializer.Serialize(message);
      var messageBytes = Encoding.UTF8.GetBytes(jsonMessage);
      return messageBytes;
    });

    return Task.Run(() => _publisher.MultiPublish(topic, messagesBytes));
  }

  public void Stop() => _publisher.Stop();
}
