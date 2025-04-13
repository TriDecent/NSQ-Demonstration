namespace Common.Logger;

public class MultiLogger(IEnumerable<ILogger> loggers) : ILogger
{
  private readonly IEnumerable<ILogger> _loggers = loggers;

  public void WriteLine(string message)
  {
    foreach (var logger in _loggers)
    {
      logger.WriteLine(message);
    }
  }
}
