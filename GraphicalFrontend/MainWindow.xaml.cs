using System.ComponentModel;
using System.Windows;
using GraphicalFrontend.Ai;
using GraphicalFrontend.Client;
using GraphicalFrontend.GameEngine;
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
      var shimocha = new PlayerViewModel(1);
      var toimen = new PlayerViewModel(2);
      var kamicha = new PlayerViewModel(3);
      _messages = new MessagesViewModel();
      var boardViewModel = new BoardViewModel(watashi, shimocha, toimen, kamicha);
      DataContext = new MainViewModel(_messages, boardViewModel);

      //var ai = new SimpleAi(Settings.Default.TenhouId, "0", false);
      var audience = new UiThreadAudience(_messages, boardViewModel, watashi, kamicha, toimen, shimocha);

      //_client = new TenhouClient(ai, audience);
      //_client.LogOn();

      //_localClient = new LocalClient(ai, audience);

      RunMatch(audience);
    }

    private static void RunMatch(ISpectator spectator)
    {
      var ai0 = new SimpleAi("A", "0", true);
      var ai1 = new SimpleAi("B", "0", true);
      var ai2 = new SimpleAi("C", "0", true);
      var ai3 = new SimpleAi("D", "0", true);
      var board = new Board();
      var decider = new Decider(board, new[] {ai0, ai1, ai2, ai3});
      Match.Start(decider, board, spectator);
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