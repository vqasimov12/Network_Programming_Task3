using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TcpServer;


var responsePort = 27000;
var requestPort = 27001;
var ip = IPAddress.Parse("10.1.18.7");
var requestEp = new IPEndPoint(ip, requestPort);
var responseEp = new IPEndPoint(ip, responsePort);
using var server = new TcpClient(requestEp);
using var listener = new TcpListener(requestPort);
listener.Start();
Console.WriteLine("Server started, waiting for connections...");

while (true)
{
    var client = listener.AcceptTcpClient();
    Console.WriteLine("Client connected.");
    _ = Task.Run(() =>
    {
        using var stream = client.GetStream();
        using var reader = new StreamReader(stream);
        try
        {
            var message = reader.ReadLine();
            if (message == "Refresh")
            {
                Console.WriteLine("Received 'Refresh' request.");
                server.Connect(responseEp);
                if (!server.Connected)
                {
                    using var writer = new StreamWriter(server.GetStream());
                    var processes = Process.GetProcesses()
                        .Select(p => new ProcessInfo
                        {
                            Id = p.Id,
                            ProcessName = p.ProcessName,
                            HandleCount = p.HandleCount,
                            ThreadCount = p.Threads.Count,
                            MachineName = p.MachineName
                        })
                        .OrderBy(p => p.ProcessName)
                        .ToList();
                    Console.WriteLine($"Sending processes to client.");
                    for (int k = 0; k < processes.Count; k++)
                    {
                        var json = JsonSerializer.Serialize(processes[k]);
                        writer.Write(json);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    });
}
