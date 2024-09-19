namespace TcpServer;
public class ProcessInfo
{
    public int Id { get; set; }
    public string ProcessName { get; set; }
    public override string ToString() => $"{Id}.  {ProcessName}";
}
