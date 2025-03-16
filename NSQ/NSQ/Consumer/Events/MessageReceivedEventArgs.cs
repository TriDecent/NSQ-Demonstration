using NSQ.Models;

namespace NSQ.Consumer.Events;

public class MessageReceivedEventArgs(Message message) : EventArgs
{
  public Message Message { get; } = message;
}
