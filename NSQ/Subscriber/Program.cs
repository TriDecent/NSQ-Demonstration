using System.Text;
using System.Text.Json;
using NsqSharp;



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

public class ConsumerWrapper(Consumer consumer) : IConsumerWrapper
{
  private readonly Consumer _consumer = consumer;

  public void AddHandler(IHandler handler, int threads = 1)
    => _consumer.AddHandler(handler, threads);

  public void ConnectToNsqLookupd(params string[] addresses)
    => _consumer.ConnectToNsqLookupd(addresses);

  public void Stop() => _consumer.Stop();
}

public interface IConsumerWrapper
{
  void AddHandler(IHandler handler, int threads = 1);
  void ConnectToNsqLookupd(params string[] addresses);
  void Stop();
}

public interface INSQConsumer
{
  Task ConsumeFromAsync(params IEnumerable<string> address);
  void Stop();
  event EventHandler<MessageReceivedEventArgs>? MessageReceived;
}

public class MessageReceivedEventArgs(Message message) : EventArgs
{
  public Message Message { get; } = message;
}

public record Message(string Sender, ReadOnlyMemory<byte> Body);

public readonly record struct Person(string Id, string Name);
