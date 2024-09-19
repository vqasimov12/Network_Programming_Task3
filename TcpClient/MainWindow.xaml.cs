using System.IO;
using System.Net;
using System.Windows;
using System.Net.Sockets;
using System.Text.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
namespace TcpClient;
public partial class MainWindow : Window, INotifyPropertyChanged
{
    private ObservableCollection<ProcessInfo> processes;
    public ObservableCollection<ProcessInfo> Processes { get => processes; set { processes = value; OnPropertyChanged(); } }
    public MainWindow()
    {
        InitializeComponent();
        processes = new();
        CBox.Items.Add("Kill");
        CBox.Items.Add("Start");
        DataContext = this;
    }
    private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        var responsePort = 27001;
        var requestPort = 27000;
        var ip = IPAddress.Parse("192.168.1.8");
        var requestEp = new IPEndPoint(ip, requestPort);
        var responseEp = new IPEndPoint(ip, responsePort);
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            _ = client.ConnectAsync(requestEp.Address, requestEp.Port);
            using (var writer = new StreamWriter(client.GetStream()))
            {
                writer.WriteLine("Refresh");
                writer.Flush();
            }
            using (var listener = new TcpListener(responseEp))
            {
                listener.Start();
                using (var responseClient = await listener.AcceptTcpClientAsync())
                {
                    using var reader = new StreamReader(responseClient.GetStream());
                    var processes = new List<ProcessInfo>();
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        var processInfo = JsonSerializer.Deserialize<ProcessInfo>(line);
                        Processes.Add(processInfo);
                    }
                }
                listener.Stop();
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