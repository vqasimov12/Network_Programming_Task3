namespace TcpClient;
public class Command
{
    public string ProcessName { get; set; }
    public string CommandType { get; set; }
    public int ProcessId { get; set; } = -1;
}
