namespace TcpServer;
public class Command
{
    public string ProcessName {  get; set; }
    public string CommandType { get; set; }
    public short ProcessId { get; set; } = -1;
}
