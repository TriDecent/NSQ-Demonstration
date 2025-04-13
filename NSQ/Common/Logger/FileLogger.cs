namespace Common.Logger;

public class FileLogger(string filePath) : ILogger
{
  private readonly string _filePath = filePath;

  public void WriteLine(string message)
  {
    File.AppendAllText(_filePath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n");
  }
}
