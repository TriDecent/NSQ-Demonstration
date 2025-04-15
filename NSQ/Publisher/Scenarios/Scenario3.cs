using System.Text;
using Common;
using NSQ.Address;
using NSQ.Factory;
using NSQ.Publisher.Scenarios;

namespace Publisher.Scenarios;

public class Scenario3 : IScenario
{
  public async Task ExecuteAsync()
  {
    var factory = new NSQFactory();
    var publisher1 = factory.CreatePublisher(NSQEndpointExtensions.GetDaemonEndpoint(), TimeSpan.FromSeconds(5));
    var publisher2 = factory.CreatePublisher(NSQEndpointExtensions.GetDaemonEndpoint(), TimeSpan.FromSeconds(5));
    var publisher3 = factory.CreatePublisher(NSQEndpointExtensions.GetDaemonEndpoint(), TimeSpan.FromSeconds(5));

    Console.WriteLine("Starting NSQ Publisher for Multiple Publishers...");
    Console.WriteLine($"Topic: {ScenarioMetadata.MULTIPLE_PUBLISHERS_TOPIC}");
    Console.WriteLine("Type a message to publish or 'exit' to stop.");

    string input = "";
    while (input is not "exit")
    {
      Console.Write("Enter message: ");
      input = Console.ReadLine()!;
      if (input is not "exit")
      {
        var tasks = new List<Task>
        {
          publisher1.PublishAsync(
            ScenarioMetadata.MULTIPLE_PUBLISHERS_TOPIC,
            new NSQ.Models.Message("Publisher-1", Encoding.UTF8.GetBytes($"{input} (from Publisher 1)"))),

          publisher2.PublishAsync(
            ScenarioMetadata.MULTIPLE_PUBLISHERS_TOPIC,
            new NSQ.Models.Message("Publisher-2", Encoding.UTF8.GetBytes($"{input} (from Publisher 2)"))),

          publisher3.PublishAsync(
            ScenarioMetadata.MULTIPLE_PUBLISHERS_TOPIC,
            new NSQ.Models.Message("Publisher-3", Encoding.UTF8.GetBytes($"{input} (from Publisher 3)")))
        };

        await Task.WhenAll(tasks);
        Console.WriteLine("[Message Sent]");
        Console.WriteLine($"  Topic: {ScenarioMetadata.MULTIPLE_PUBLISHERS_TOPIC}");
        Console.WriteLine($"  Message: {input}");
        Console.WriteLine(new string('-', 50));
      }
    }

    publisher1.Stop();
    publisher2.Stop();
    publisher3.Stop();
    Console.WriteLine("Publishers stopped. Exiting...");
  }
}
