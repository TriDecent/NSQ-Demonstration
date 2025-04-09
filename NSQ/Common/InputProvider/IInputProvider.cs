namespace Common.InputProvider;

public interface IInputProvider
{
  int GetInt(string prompt, int defaultValue);
  void WaitForUser(string message);
}