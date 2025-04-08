namespace NSQ.Publisher.Scenarios;

using System.Text;
using NSQ.Factory;

public class Scenario : IScenario
{
  public async Task ExecuteAsync()
  {
    var publisher = new NSQFactory().CreatePublisher("127.0.0.1:4151", TimeSpan.FromSeconds(5));

    var input = "hello from publisher";

    while (input is not "exit")
    {
      await publisher.PublishAsync(
        "topic_test",
        new Models.Message("Trí", Encoding.UTF8.GetBytes(input!)));

      input = Console.ReadLine();
    }
  }
}
