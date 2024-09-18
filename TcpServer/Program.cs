using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using TcpServer;


var responsePort = 27002;
var requestPort = 27001;
var ip = IPAddress.Parse("192.168.1.8");
var requestEp = new IPEndPoint(ip, requestPort);
var responseEp = new IPEndPoint(ip, responsePort);
using var server = new TcpClient(requestEp);
using var listener = new TcpListener(requestPort);
listener.Start();
Console.WriteLine("Server started, waiting for connections...");
/*
while (true)
{
    try
    {
        var client = listener.AcceptTcpClient();
        Console.WriteLine("Client connected.");
        
        _ = Task.Run(() =>
         {
             using var stream = client.GetStream();
             using var sr = new StreamReader(stream);
             using var sw = new StreamWriter(stream) { AutoFlush = true };

             try
             {
                 var message = sr.ReadLine();
                 if (message == "Refresh")
                 {
                     Console.WriteLine("Received 'Refresh' request.");

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
                     int i = 0;
                     Console.WriteLine($"Sending processes to client.");
                     while (i < processes.Count)
                     {
                         for (int k = 0; k < 10; k++)
                         {
                             var json = JsonSerializer.Serialize(processes[k]);
                             sw.WriteLine(json);
                             sw.Flush();
                         }
                         i += 10;
                     }



                     Console.WriteLine("Process list sent to client.");
                 }
             }

             catch (Exception ex)
             {
                 Console.WriteLine($"Error: {ex.Message}");
             }
             finally
             {
                 client.Close();
                 Console.WriteLine("Client connection closed.");
             }
         });
    }
    catch (SocketException se)
    {
        Console.WriteLine($"SocketException: {se.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }
}*/

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
                    using var writer= new StreamWriter(server.GetStream());
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
                    int i = 0;
                    Console.WriteLine($"Sending processes to client.");
                    while (i < processes.Count)
                    {
                        for (int k = 0; k < 10; k++)
                        {
                            var json = JsonSerializer.Serialize(processes[k]);
                            writer.Write(json);
                        }
                        i += 10;
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
