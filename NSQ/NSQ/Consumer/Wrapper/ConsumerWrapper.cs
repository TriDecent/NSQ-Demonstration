namespace NSQ.Consumer.Wrapper;
using NsqSharp;

internal class ConsumerWrapper(Consumer consumer) : IConsumerWrapper
{
  private readonly Consumer _consumer = consumer;

  public void AddHandler(IHandler handler, int threads = 1)
    => _consumer.AddHandler(handler, threads);

  public void ConnectToNsqLookupd(params string[] addresses)
    => _consumer.ConnectToNsqLookupd(addresses);

  public void Stop() => _consumer.Stop();
}
