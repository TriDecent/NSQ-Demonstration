
namespace Common.InputProvider;

public class ConsoleInputProvider : IInputProvider
{
  public int GetInt(string prompt, int defaultValue)
  {
    Console.Write($"{prompt} (default {defaultValue}): ");
    if (!int.TryParse(Console.ReadLine(), out int value) || value <= 0)
      return defaultValue;
  
    return value;
  }

  public void WaitForUser(string message)
  {
    Console.WriteLine(message);
    Console.ReadLine();
  }
}