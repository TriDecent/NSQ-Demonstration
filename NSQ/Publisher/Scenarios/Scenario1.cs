namespace NSQ.Publisher.Scenarios;

using System.Text;
using NSQ.Publisher;
using NsqSharp.Bus.Configuration.BuiltIn;

public class Scenario1 : IScenario
{
  public async Task ExecuteAsync()
  {
    var publisher = new NSQPublisher(new NsqdHttpPublisher("127.0.0.1:4151", TimeSpan.FromSeconds(5)));

    var input = "hello from publisher";

    while (input is not "exit")
    {
      await publisher.PublishAsync(
        "topic_test",
        new NSQ.Models.Message("Trí", Encoding.UTF8.GetBytes(input!)));

      input = Console.ReadLine();
    }
  }
}
