using System.Collections.ObjectModel;
using GraphicalFrontend.Client;
using GraphicalFrontend.GameEngine;

namespace GraphicalFrontend.ViewModels
{
  internal class BoardViewModel : ViewModelBase, ISpectator
  {
    public BoardViewModel(PlayerViewModel watashi, PlayerViewModel shimocha, PlayerViewModel toimen, PlayerViewModel kamicha)
    {
      Watashi = watashi;
      Shimocha = shimocha;
      Kamicha = kamicha;
      Toimen = toimen;
    }

    public PlayerViewModel Watashi { get; }

    public PlayerViewModel Shimocha { get; }

    public PlayerViewModel Toimen { get; }

    public PlayerViewModel Kamicha { get; }


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

    public void Updated(VisibleBoard board)
    {
      DoraIndicators.Clear();
      foreach (var tile in board.DoraIndicators)
      {
        DoraIndicators.Add(tile.TileId);
      }
    }
  }
}