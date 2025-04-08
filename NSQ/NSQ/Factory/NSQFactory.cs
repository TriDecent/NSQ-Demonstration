
using NSQ.Consumer;
using NSQ.Consumer.Wrapper;
using NSQ.Models;
using NSQ.Publisher;
using NsqSharp.Bus.Configuration.BuiltIn;

namespace NSQ.Factory;

public class NSQFactory : INSQFactory
{
  public INSQConsumer CreateConsumer(string topic, string channel, Action<string, Message> messageHandler)
  {
    var consumerWrapper = new ConsumerWrapper(new NsqSharp.Consumer(topic, channel));
    var nsqConsumer = new NSQConsumer(consumerWrapper);

    nsqConsumer.MessageReceived += (_, e)
      => messageHandler.Invoke(e.Message.Sender, e.Message);

    return nsqConsumer;
  }

  public INSQPublisher CreatePublisher(string nsqdAddress, TimeSpan timeout)
  {
    var nsqdPublisher = new NsqdHttpPublisher(nsqdAddress, timeout);
    var publisher = new NSQPublisher(nsqdPublisher);

    return publisher;
  }
}
