using System.Collections.ObjectModel;
using System.Linq;
using Game.Shared;

namespace GraphicalFrontend.ViewModels
{
  internal class PlayerViewModel : ViewModelBase, ISpectator
  {
    public PlayerViewModel(int seatIndex)
    {
      _seatIndex = seatIndex;
    }

    public ObservableCollection<int> ConcealedTiles { get; } = new();

    public string Wind
    {
      get => _wind;
      set
      {
        _wind = value;
        OnPropertyChanged();
      }
    }

    public bool HasRecentDraw
    {
      get => _hasRecentDraw;
      private set
      {
        _hasRecentDraw = value;
        OnPropertyChanged();
      }
    }

    public bool HasDeclaredRiichi
    {
      get => _hasDeclaredRiichi;
      private set
      {
        _hasDeclaredRiichi = value;
        OnPropertyChanged();
      }
    }

    public ObservableCollection<MeldViewModel> Melds { get; } = new();

    public string Name => _seatIndex.ToString();
    
    public ObservableCollection<int> PondRow0 { get; } = new();

    public ObservableCollection<int> PondRow1 { get; } = new();

    public ObservableCollection<int> PondRow2 { get; } = new();

    public string Rank => "god";

    public string Rate => "<1800";

    public int RecentDraw
    {
      get => _recentDraw;
      private set
      {
        _recentDraw = value;
        OnPropertyChanged();
      }
    }

    public bool Riichi
    {
      get => _riichi;
      private set
      {
        _riichi = value;
        OnPropertyChanged();
      }
    }

    public string Score
    {
      get => _score;
      private set
      {
        _score = value;
        OnPropertyChanged();
      }
    }

    public void Sent(string message)
    {
    }

    public void Error(string message)
    {
    }

    public void Received(string message)
    {
    }

    public void Updated(VisibleBoard board)
    {
      var seat = board.Seats[_seatIndex];
      HasDeclaredRiichi = seat.DeclaredRiichi;

      var ordered = seat.ConcealedTiles.OrderBy(x => x.TileId).ToList();
      if (seat.CurrentDraw != null)
      {
        ordered.Remove(seat.CurrentDraw);
      }

      RecentDraw = seat.CurrentDraw?.TileId ?? -1;
      HasRecentDraw = seat.CurrentDraw != null;

      ConcealedTiles.Clear();
      foreach (var tile in ordered)
      {
        ConcealedTiles.Add(tile.TileId);
      }

      Melds.Clear();
      foreach (var meld in seat.Melds)
      {
        Melds.Add(new MeldViewModel(meld.Tiles));
      }

      Riichi = seat.DeclaredRiichi;
      Score = $"{seat.Score}";
      Wind = "東南西北".Substring(seat.SeatWind.TileTypeId - 27, 1);

      UpdatePond(seat);
    }

    private readonly int _seatIndex;
    private bool _hasRecentDraw;
    private bool _hasDeclaredRiichi;
    private int _recentDraw;
    private bool _riichi;
    private string _score = "";
    private string _wind = "";
    
    private void UpdatePond(VisiblePlayer seat)
    {
      PondRow0.Clear();
      PondRow1.Clear();
      PondRow2.Clear();
      for (var index = 0; index < 6 && index < seat.Discards.Count; index++)
      {
        var tile = seat.Discards[index];
        PondRow0.Add(tile.TileId);
      }

      for (var index = 6; index < 12 && index < seat.Discards.Count; index++)
      {
        var tile = seat.Discards[index];
        PondRow1.Add(tile.TileId);
      }

      for (var index = 12; index < seat.Discards.Count; index++)
      {
        var tile = seat.Discards[index];
        PondRow2.Add(tile.TileId);
      }
    }
  }
}