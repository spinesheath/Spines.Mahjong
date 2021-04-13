using System.Collections.ObjectModel;
using GraphicalFrontend.Client;

namespace GraphicalFrontend.ViewModels
{
  internal class BoardViewModel : ViewModelBase, ISpectator
  {
    public BoardViewModel(PlayerViewModel watashi, PlayerViewModel kamicha, PlayerViewModel toimen, PlayerViewModel shimocha)
    {
      Watashi = watashi;
      Kamicha = kamicha;
      Toimen = toimen;
      Shimocha = shimocha;
    }

    public PlayerViewModel Watashi { get; }

    public PlayerViewModel Kamicha { get; }

    public PlayerViewModel Toimen { get; }

    public PlayerViewModel Shimocha { get; }

    public ObservableCollection<int> DoraIndicators { get; } = new ();

    public void Sent(string message)
    {
    }

    public void Error(string message)
    {
    }

    public void Received(string message)
    {
    }

    public void Updated(IGameState state)
    {
      DoraIndicators.Clear();
      foreach (var tile in state.DoraIndicators)
      {
        DoraIndicators.Add(tile.TileId);
      }
    }
  }
}