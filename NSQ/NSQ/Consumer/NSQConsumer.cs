namespace NSQ.Consumer;

using System.Text.Json;
using NSQ.Consumer.Events;
using NSQ.Consumer.Wrapper;
using NSQ.Models;

using NsqSharp;
using Message = Models.Message;

public class NSQConsumer(IConsumerWrapper consumer) : INSQConsumer
{
  private readonly IConsumerWrapper _consumer = consumer;
  public event EventHandler<MessageReceivedEventArgs>? MessageReceived;

  public Task ConsumeFromAsync(params IEnumerable<string> address)
  {
    _consumer.AddHandler(new MessageHandler(this));

    return Task.Run(() => _consumer.ConnectToNsqLookupd([.. address]));
  }

  private void OnMessageReceived(Message message)
    => MessageReceived?.Invoke(this, new MessageReceivedEventArgs(message));

  public void Stop() => _consumer.Stop();


  private class MessageHandler(NSQConsumer parent) : IHandler
  {
    private readonly NSQConsumer _parent = parent;

    public void HandleMessage(IMessage message)
    {
      var originalMessage = JsonSerializer.Deserialize<Message>(message.Body)!;
      _parent.OnMessageReceived(originalMessage);
    }

    public void LogFailedMessage(IMessage message)
    {
      // Just dont call this
      throw new NotImplementedException();
    }
  }
}
