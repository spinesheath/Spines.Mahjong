using System.ComponentModel;
using System.Windows;
using GraphicalFrontend.Ai;
using GraphicalFrontend.Client;
using GraphicalFrontend.Properties;
using GraphicalFrontend.ViewModels;

namespace GraphicalFrontend
{
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();

      var messages = new MessagesViewModel();
      var board = new BoardViewModel();
      DataContext = new MainViewModel(messages, board);

      var ai = new SimpleAi(Settings.Default.TenhouId, "0");
      var audience = new Audience(messages, board);

      _client = new TenhouClient(ai, audience);
      _client.LogOn();
    }

    private TenhouClient? _client;

    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);

      _client?.Dispose();
      _client = null;
    }

    private void StartTestplay(object sender, RoutedEventArgs e)
    {
      _client?.Testplay();
    }
  }
}