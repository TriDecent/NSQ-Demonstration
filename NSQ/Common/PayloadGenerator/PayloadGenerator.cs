namespace Common.PayloadGenerator;

public interface IPayloadGenerator
{
  ReadOnlyMemory<byte> GeneratePayload(int sizeInBytes);
}