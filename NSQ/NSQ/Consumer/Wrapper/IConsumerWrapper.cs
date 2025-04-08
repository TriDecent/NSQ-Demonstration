namespace NSQ.Consumer.Wrapper;
using NsqSharp;

internal interface IConsumerWrapper
{
  void AddHandler(IHandler handler, int threads = 1);
  void ConnectToNsqLookupd(params string[] addresses);
  void Stop();
}
