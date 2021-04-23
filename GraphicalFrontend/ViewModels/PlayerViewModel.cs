using System.Collections.ObjectModel;
using System.Linq;
using GraphicalFrontend.Client;
using GraphicalFrontend.GameEngine;

namespace GraphicalFrontend.ViewModels
{
  internal class PlayerViewModel : ViewModelBase, ISpectator
  {
    public PlayerViewModel(int seatIndex)
    {
      _seatIndex = seatIndex;
    }

    public ObservableCollection<int> ConcealedTiles { get; } = new();

    public string Direction
    {
      get => _direction;
      set
      {
        _direction = value;
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

    public void Updated(IGameState state)
    {
      if (_seatIndex == 0)
      {
        HasDeclaredRiichi = state.DeclaredRiichi;

        var ordered = state.ConcealedTiles.OrderBy(x => x.TileId).ToList();
        if (state.RecentDraw != null)
        {
          ordered.Remove(state.RecentDraw);
        }

        RecentDraw = state.RecentDraw?.TileId ?? -1;
        HasRecentDraw = state.RecentDraw != null;
        
        ConcealedTiles.Clear();
        foreach (var tile in ordered)
        {
          ConcealedTiles.Add(tile.TileId);
        }

        Melds.Clear();
        foreach (var meld in state.Melds)
        {
          Melds.Add(new MeldViewModel(meld.Tiles));
        }

        Riichi = state.DeclaredRiichi;
        Score = $"{state.Score}00";
        Direction = "東南西北".Substring(state.SeatWind.TileTypeId - 27, 1);

        UpdatePond(state);
      }
      else
      {
        ConcealedTiles.Clear();
        for (var i = 0; i < 13; i++)
        {
          ConcealedTiles.Add(-1);
        }

        //Melds.Clear();
        //foreach (var meld in state.)
        //{
        //  Melds.Add(new MeldViewModel(meld.Tiles));
        //}

        //Riichi = state.DeclaredRiichi;
        //Score = $"{state.Score}00";
        //Direction = "東南西北".Substring(state.SeatWind.TileTypeId - 27, 1);

        UpdatePond(state);
      }
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
      Direction = "東南西北".Substring(seat.SeatWind.TileTypeId - 27, 1);

      UpdatePond(seat);
    }

    private readonly int _seatIndex;
    private bool _hasRecentDraw;
    private bool _hasDeclaredRiichi;
    private int _recentDraw;
    private bool _riichi;
    private string _score = "";
    private string _direction = "";

    private void UpdatePond(IGameState state)
    {
      PondRow0.Clear();
      PondRow1.Clear();
      PondRow2.Clear();
      for (var index = 0; index < 6 && index < state.Ponds[_seatIndex].Count; index++)
      {
        var tile = state.Ponds[_seatIndex][index];
        PondRow0.Add(tile.Tile.TileId);
      }

      for (var index = 6; index < 12 && index < state.Ponds[_seatIndex].Count; index++)
      {
        var tile = state.Ponds[_seatIndex][index];
        PondRow1.Add(tile.Tile.TileId);
      }

      for (var index = 12; index < state.Ponds[_seatIndex].Count; index++)
      {
        var tile = state.Ponds[_seatIndex][index];
        PondRow2.Add(tile.Tile.TileId);
      }
    }

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