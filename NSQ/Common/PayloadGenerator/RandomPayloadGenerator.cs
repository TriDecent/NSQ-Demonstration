namespace Common.PayloadGenerator;

public class RandomPayloadGenerator : IPayloadGenerator
{
  private readonly Random _random;

  public RandomPayloadGenerator() => _random = new Random();

  public ReadOnlyMemory<byte> GeneratePayload(int sizeInBytes)
  {
    var buffer = new byte[sizeInBytes];
    _random.NextBytes(buffer);
    return buffer;
  }
}

