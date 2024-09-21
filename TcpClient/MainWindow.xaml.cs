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
        RefreshBtn_Click(null, null);

    }
    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        var responsePort = 27001;
        var requestPort = 27000;
        var ip = IPAddress.Parse("192.168.1.8");
        var requestEp = new IPEndPoint(ip, requestPort);
        var responseEp = new IPEndPoint(ip, responsePort);
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            client.Connect(requestEp.Address, requestEp.Port);
            var command = new Command() { CommandType = "Refresh" };
            var json = JsonSerializer.Serialize(command);
            using (var writer = new StreamWriter(client.GetStream()))
            {
                writer.WriteLine(json);
                writer.Flush();
            }
            using (var listener = new TcpListener(responseEp))
            {
                listener.Start();
                using (var responseClient = listener.AcceptTcpClient())
                {
                    using var reader = new StreamReader(responseClient.GetStream());
                    string line;
                    Processes.Clear();
                    while ((line = reader.ReadLine()) != null)
                    {
                        var processInfo = JsonSerializer.Deserialize<ProcessInfo>(line);
                        Processes.Add(processInfo!);
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

    private void RunBtn_Click(object sender, RoutedEventArgs e)
    {
        var command = new Command();
        if (CBox.SelectedIndex == 0)
        {
            var process = List.SelectedItem as ProcessInfo;
            if (process is null) return;
            command.ProcessId = process.Id;
            command.CommandType = "Kill";
        }
        else
        {
            if (ProcessName.Text == "") return;
            command.ProcessName = ProcessName.Text.ToString();
            command.CommandType = "Start";
        }
        var responsePort = 27001;
        var requestPort = 27000;
        var ip = IPAddress.Parse("192.168.1.8");
        var requestEp = new IPEndPoint(ip, requestPort);
        var responseEp = new IPEndPoint(ip, responsePort);
        try
        {
            using var client = new System.Net.Sockets.TcpClient();
            client.Connect(requestEp.Address, requestEp.Port);
            var json = JsonSerializer.Serialize(command);
            using (var writer = new StreamWriter(client.GetStream()))
            {
                writer.WriteLine(json);
                writer.Flush();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message);
        }
        CBox.SelectedIndex = 0;
        ProcessName.Text = "";
        RefreshBtn_Click(null, null);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


}