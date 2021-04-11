using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GraphicalFrontend.Client;

namespace GraphicalFrontend.ViewModels
{
  internal class BoardViewModel : ViewModelBase, ISpectator
  {
    public ObservableCollection<int> Hand
    {
      get => _hand;
      set
      {
        _hand = value;
        OnPropertyChanged();
      }
    }

    public OpponentViewModel Kamicha { get; }

    public ObservableCollection<MeldViewModel> Melds
    {
      get => _melds;
      set
      {
        _melds = value;
        OnPropertyChanged();
      }
    }

    public OpponentViewModel Shimocha { get; }

    public OpponentViewModel Toimen { get; }

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
      var ordered = state.ConcealedTileIds.OrderBy(x => x).ToList();
      // Move recent draw to the end.
      if (state.RecentDraw != null)
      {
        ordered.Remove(state.RecentDraw.Value);
        ordered.Add(state.RecentDraw.Value);
      }

      Hand = new ObservableCollection<int>(ordered);
      var melds = new ObservableCollection<MeldViewModel>();
      foreach (var meld in state.Melds)
      {
        melds.Add(new MeldViewModel(meld.Tiles));
      }

      Melds = melds;
    }

    private ObservableCollection<int> _hand = new();
    private ObservableCollection<MeldViewModel> _melds = new();
  }

  internal class MeldViewModel : ViewModelBase
  {
    public MeldViewModel(IEnumerable<int> tiles)
    {
      Tiles = new ObservableCollection<int>(tiles);
    }

    public ObservableCollection<int> Tiles { get; }
  }
}