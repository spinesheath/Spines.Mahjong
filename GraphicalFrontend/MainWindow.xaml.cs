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

      var watashi = new PlayerViewModel(0);
      var kamicha = new PlayerViewModel(1);
      var toimen = new PlayerViewModel(2);
      var shimocha = new PlayerViewModel(3);
      _messages = new MessagesViewModel();
      var board = new BoardViewModel(watashi, kamicha, toimen, shimocha);
      DataContext = new MainViewModel(_messages, board);

      var ai = new SimpleAi(Settings.Default.TenhouId, "0", false);
      var audience = new UiThreadAudience(_messages, board, watashi, kamicha, toimen, shimocha);

      //_client = new TenhouClient(ai, audience);
      //_client.LogOn();

      _localClient = new LocalClient(ai, audience);
    }

    private TenhouClient? _client;
    private readonly LocalClient _localClient;
    private readonly MessagesViewModel _messages;

    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);

      _client?.Dispose();
      _client = null;
    }

    private void StartTestplay(object sender, RoutedEventArgs e)
    {
      _messages.Clear();
      _localClient.Start();
      //_client?.Testplay();
    }

    private void StartIppan(object sender, RoutedEventArgs e)
    {
      //_client?.Ippan();
    }
  }
}