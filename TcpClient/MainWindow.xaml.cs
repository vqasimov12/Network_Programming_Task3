using System.IO;
using System.Net;
using System.Windows;
using System.Net.Sockets;
using System.Text.Json;
using System.Diagnostics;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
namespace TcpClient;
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private List<ProcessInfo> processes;

    public List<ProcessInfo> Processes { get => processes; set { processes = value; OnPropertyChanged(); } }
    public MainWindow()
    {
        InitializeComponent();
        CBox.Items.Add("Kill");
        CBox.Items.Add("Start");
        DataContext = this;
    }


    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        var responsePort = 27001;
        var requestPort = 27002;
        var ip = IPAddress.Parse("192.168.1.8");
        var requestEp = new IPEndPoint(ip, requestPort);
        var responseEp = new IPEndPoint(ip, responsePort);
        using var client = new System.Net.Sockets.TcpClient();
        using var listener = new TcpListener(requestPort);

        /*   using (System.Net.Sockets.TcpClient client = new())
           {
               var ep = new IPEndPoint(ip, port);

               try
               {
                   client.Connect(ep);
               }
               catch (SocketException ex)
               {
                   MessageBox.Show($"Could not connect to server: {ex.Message}");
                   return;
               }

               if (client.Connected)
               {
                   using (var sw = new StreamWriter(client.GetStream()))
                   {
                       sw.WriteLine("Refresh");
                       sw.Flush(); 
                   }

                   using (var sr = new StreamReader(client.GetStream()))
                   {
                       try
                       {
                           var response = sr.ReadLine();
                           if (!string.IsNullOrEmpty(response))
                           {
                               var json = JsonSerializer.Deserialize<List<ProcessInfo>>(response);
                               Processes = new(json);
                           }
                           else
                           {
                               MessageBox.Show("No response from the server.");
                           }
                       }
                       catch (IOException ex)
                       {
                           MessageBox.Show($"Error reading response: {ex.Message}");
                       }
                   }
               }
               else
               {
                   MessageBox.Show("Client is not connected.");
               }
           }*/

        try
        {
            client.Connect(responseEp);
            using (var sw = new StreamWriter(client.GetStream()))
            {
                sw.WriteLine("Refresh");
                sw.Flush();
            }
            listener.Start();
            var server = listener.AcceptTcpClient();
            using var reader = server.GetStream();
            Processes = new();
            using (var sr = new StreamReader(server.GetStream()))
            {
                while (true)
                {
                    try
                    {
                        var response = sr.ReadLine();
                        if (!string.IsNullOrEmpty(response))
                        {
                            var json = JsonSerializer.Deserialize<ProcessInfo>(response);
                            Processes.Add(json);
                        }
                        else
                        {
                            MessageBox.Show("No response from the server.");
                        }
                    }

                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}