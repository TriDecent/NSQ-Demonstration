namespace Common.Logger;

public class ConsoleLogger : ILogger
{
  public void WriteLine(string message) => Console.WriteLine(message);
}