namespace AuthGatun.Models;

public class ServiceKey
{
    public int Id { get; set; }
    public required string UserName { get; set; }
    public required string ServiceName { get; set; }
    public required string SecretKey { get; set; }
}