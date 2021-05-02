using System.ComponentModel;
using System.Windows;
using Game.Engine;
using Game.Shared;
using GraphicalFrontend.ViewModels;
using SimpleAi;

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

      var audience = new UiThreadAudience(_messages, boardViewModel, watashi, kamicha, toimen, shimocha);

      //var ai = new SimpleAi(Settings.Default.TenhouId, "0", false);
      //_client = new TenhouClient(ai, audience);
      //_client.LogOn();

      //_localClient = new LocalClient(ai, audience);

      RunMatch(audience);
    }

    private static async void RunMatch(ISpectator spectator)
    {
      var ai0 = new Delayed(new SimpleAi.SimpleAi("A", "0"));
      var ai1 = new Delayed(new SimpleAi.SimpleAi("B", "0"));
      var ai2 = new Delayed(new SimpleAi.SimpleAi("C", "0"));
      var ai3 = new Delayed(new SimpleAi.SimpleAi("D", "0"));
      //var ai0 = new Delayed(new KanAi());
      //var ai1 = new Delayed(new KanAi());
      //var ai2 = new Delayed(new KanAi());
      //var ai3 = new Delayed(new KanAi());
      await Match.Start(ai0, ai1, ai2, ai3, spectator);
    }

    //private TenhouClient? _client;
    private readonly MessagesViewModel _messages;

    protected override void OnClosing(CancelEventArgs e)
    {
      base.OnClosing(e);

      //_client?.Dispose();
      //_client = null;
    }

    private void StartTestplay(object sender, RoutedEventArgs e)
    {
      _messages.Clear();
      //_client?.Testplay();
    }

    private void StartIppan(object sender, RoutedEventArgs e)
    {
      //_client?.Ippan();
    }
  }
}