namespace NSQ.Address;

public static class NSQEndpointExtensions
{
  public static string GetLookupdEndpoint() => "127.0.0.1:4161";
  public static string GetDaemonEndpoint() => "127.0.0.1:4151";
}
