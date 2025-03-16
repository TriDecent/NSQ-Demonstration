namespace NSQ.Models;

public record Message(string Sender, ReadOnlyMemory<byte> Body);
