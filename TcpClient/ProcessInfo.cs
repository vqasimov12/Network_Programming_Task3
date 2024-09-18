namespace TcpClient;

public class ProcessInfo
{
    public int Id { get; set; }
    public string ProcessName { get; set; }
    public int HandleCount { get; set; }
    public int ThreadCount { get; set; }
    public string MachineName { get; set; }
}
