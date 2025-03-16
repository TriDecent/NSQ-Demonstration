using NSQ.Consumer.Events;

namespace NSQ.Consumer;

public interface INSQConsumer
{
  Task ConsumeFromAsync(params IEnumerable<string> address);
  void Stop();
  event EventHandler<MessageReceivedEventArgs>? MessageReceived;
}
