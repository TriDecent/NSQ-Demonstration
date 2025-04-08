using NSQ.Consumer;
using NSQ.Models;
using NSQ.Publisher;

namespace NSQ.Factory;

public interface INSQFactory
{
  INSQConsumer CreateConsumer(
    string topic,
    string channel,
    Action<string, Message> messageHandler);

  INSQPublisher CreatePublisher(
    string nsqdAddress,
    TimeSpan timeout);
}
