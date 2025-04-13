using System.Diagnostics;
using Common.Logger;

namespace Consumer.Scenario4Helpers;

public class PerformanceMeasurer(ILogger logger)
{
  private const int INACTIVITY_TIMEOUT_MS = 3000;

  private readonly ILogger _logger = logger;
  private readonly Stopwatch _stopwatch = new();
  private volatile bool _isFirstMessageReceived = false;
  private readonly Lock _firstMessageLock = new();
  private long _lastMessageReceivedTicks;
  private volatile bool _isStopwatchRunning = false;
  private readonly Lock _stopwatchLock = new();

  public bool HasStarted => _isFirstMessageReceived;
  public bool IsRunning => _isStopwatchRunning;

  public void NotifyMessageReceived()
  {
    Interlocked.Exchange(ref _lastMessageReceivedTicks, DateTime.Now.Ticks);

    if (!_isFirstMessageReceived)
    {
      lock (_firstMessageLock)
      {
        if (!_isFirstMessageReceived)
        {
          _stopwatch.Start();
          _isStopwatchRunning = true;
          _isFirstMessageReceived = true;
          _logger.WriteLine("First message received! Starting performance measurement at 0.000s");
        }
      }
    }
    else if (!_isStopwatchRunning)
    {
      lock (_stopwatchLock)
      {
        if (!_isStopwatchRunning)
        {
          double resumeAtSeconds = _stopwatch.Elapsed.TotalSeconds;
          _stopwatch.Start();
          _isStopwatchRunning = true;
          _logger.WriteLine($"Messages resumed - Restarting timer at {resumeAtSeconds:F3}s");
        }
      }
    }
  }

  public void CheckInactivity()
  {
    var currentTicks = DateTime.Now.Ticks;
    var lastTicks = Interlocked.Read(ref _lastMessageReceivedTicks);
    var elapsedMs = (currentTicks - lastTicks) / TimeSpan.TicksPerMillisecond;

    if (elapsedMs > INACTIVITY_TIMEOUT_MS)
    {
      lock (_stopwatchLock)
      {
        if (_isStopwatchRunning)
        {
          double stoppedAtSeconds = _stopwatch.Elapsed.TotalSeconds;
          _stopwatch.Stop();
          _isStopwatchRunning = false;
          _logger.WriteLine($"No messages received for {INACTIVITY_TIMEOUT_MS}ms - Pausing timer at {stoppedAtSeconds:F3}s");
        }
      }
    }
  }

  public void Stop()
  {
    lock (_stopwatchLock)
    {
      if (_isStopwatchRunning)
      {
        _stopwatch.Stop();
        _isStopwatchRunning = false;
      }
    }
  }

  public TimeSpan ElapsedTime => _stopwatch.Elapsed;
}
