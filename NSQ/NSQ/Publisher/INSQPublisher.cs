using NSQ.Models;

namespace NSQ.Publisher;

public interface INSQPublisher
{
  public Task PublishAsync(string topic, Message message);
  public Task MultiPublishAsync(string topic, IEnumerable<Message> messages);
  public void Stop();
}
