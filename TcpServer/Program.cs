using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TcpServer;

var responsePort = 27001;
var requestPort = 27000;
var ip = IPAddress.Parse("192.168.1.8");
var requestEp = new IPEndPoint(ip, requestPort);
var responseEp = new IPEndPoint(ip, responsePort);
using var listener = new TcpListener(requestEp);
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
            var json = reader.ReadLine();
            var command = JsonSerializer.Deserialize<Command>(json);
          
            if (command.CommandType == "Refresh")
            {
                Console.WriteLine("Received 'Refresh' request.");
                using var responseClient = new TcpClient();
                responseClient.Connect(responseEp);
                using var writer = new StreamWriter(responseClient.GetStream());
                var processes = Process.GetProcesses()
                    .Select(p => new ProcessInfo
                    {
                        Id = p.Id,
                        ProcessName = p.ProcessName,
                    })
                    .OrderBy(p => p.ProcessName).OrderBy(p => p.ProcessName)
                    .ToList();
                Console.WriteLine($"Sending {processes.Count} processes to client.");
                foreach (var process in processes)
                {
                    var jsontext = JsonSerializer.Serialize(process);
                    writer.WriteLine(jsontext);
                    writer.Flush();
                }
                Console.WriteLine("Finished sending processes.");
            }

            else if (command.CommandType == "Start")
            {
                Console.WriteLine("Received 'Start' request.");
                Process.Start(command.ProcessName);
                Console.WriteLine("Start Process executed");
            }
         
            else if (command.CommandType == "Kill")
            {
                Console.WriteLine("Received 'Kill' request.");
                var p = Process.GetProcessById(command.ProcessId);
                p?.Kill();
                Console.WriteLine("Kill Process executed");
            }
      
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally { client.Close(); }
    });
}