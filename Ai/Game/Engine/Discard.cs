using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Discard : State
  {
    public Discard(Tile tile)
    {
      _tile = tile;
    }

    public override State Advance()
    {
      if (_ignoredRonSeats.Any())
      {
        return new IgnoredRon(_ignoredRonSeats, _nextState!);
      }

      return _nextState!;
    }

    public override async Task Decide(Board board, Decider decider)
    {
      if (_tile.TileType.Suit == Suit.Jihai && _tile.Index < 4 && board.IsFirstGoAround)
      {
        if (board.Seats.SelectMany(s => s.Discards).Count(t => t.TileType == _tile.TileType) == 4)
        {
          _nextState = new Abort();
          return;
        }
      }

      var fourKanAbortIfNoRon = board.Seats.SelectMany(s => s.Melds).Count(m => m.IsKan) == 4 && board.Seats.Count(s => s.Melds.Any(m => m.IsKan)) > 1;
        
      var reactionTasks = new Task<DiscardResponse>[4];
      var clients = new Client[4];
      for (var i = 0; i < 4; i++)
      {
        if (i == board.ActiveSeatIndex)
        {
          reactionTasks[i] = Task.FromResult(DiscardResponse.Pass());
          clients[i] = new Client(i, DiscardActions.Pass);
          continue;
        }
        
        var actions = GetPossibleActions(board, i, fourKanAbortIfNoRon);
        if (actions != DiscardActions.Pass)
        {
          reactionTasks[i] = decider.OnDiscard(actions, i);
          clients[i] = new Client(i, actions);
          continue;
        }

        reactionTasks[i] = Task.FromResult(DiscardResponse.Pass());
        clients[i] = new Client(i, DiscardActions.Pass);
      }

      await Task.WhenAll(reactionTasks);

      for (var i = 0; i < 4; i++)
      {
        reactionTasks[i].Result.Execute(clients[i]);
        if (clients[i].IgnoredRon)
        {
          _ignoredRonSeats.Add(i);
        }
      }

      var ronCount = clients.Count(c => c.Ron);
      if (ronCount == 3)
      {
        _nextState = new Abort();
        return;
      }

      if (ronCount > 0)
      {
        _nextState = new Ron(clients.Where(r => r.Ron).Select(r => r.SeatIndex));
        return;
      }
      
      if (fourKanAbortIfNoRon)
      {
        _nextState = new Abort();
        return;
      }

      var kan = clients.FirstOrDefault(c => c.Daiminkan);
      if (kan != null)
      {
        _nextState = new Daiminkan(kan.SeatIndex);
        return;
      }

      var pon = clients.FirstOrDefault(c => c.Pon);
      if (pon != null)
      {
        _nextState = new Pon(pon.SeatIndex, pon.Tile0!, pon.Tile1!, pon.Discard!);
        return;
      }

      var chii = clients.FirstOrDefault(c => c.Chii);
      if (chii != null)
      {
        _nextState = new Chii(chii.SeatIndex, chii.Tile0!, chii.Tile1!, chii.Discard!);
        return;
      }

      if (board.Wall.RemainingDraws == 0)
      {
        _nextState = new ExhaustiveDraw();
      }
      else
      {
        _nextState = new Draw((board.ActiveSeatIndex + 1) % 4);
      }
    }

    public override void Update(Board board, Wall wall)
    {
      board.ActiveSeat.Discard(_tile);
    }

    private readonly List<int> _ignoredRonSeats = new();
    private readonly Tile _tile;
    private State? _nextState;

    private DiscardActions GetPossibleActions(Board board, int seatIndex, bool fourKanAbortIfNoRon)
    {
      var suggestedActions = DiscardActions.Pass;
      suggestedActions |= CanRon(board, seatIndex) ? DiscardActions.Ron : DiscardActions.Pass;
      if (!fourKanAbortIfNoRon)
      {
        suggestedActions |= CanChii(board, seatIndex) ? DiscardActions.Chii : DiscardActions.Pass;
        suggestedActions |= CanPon(board, seatIndex) ? DiscardActions.Pon : DiscardActions.Pass;
        suggestedActions |= CanKan(board, seatIndex) ? DiscardActions.Kan : DiscardActions.Pass;
      }
      return suggestedActions;
    }

    private bool CanKan(Board board, int seatIndex)
    {
      if (board.Seats.SelectMany(s => s.Melds).Count(m => m.IsKan) == 4)
      {
        return false;
      }

      var seat = board.Seats[seatIndex];
      return !seat.DeclaredRiichi && seat.ConcealedTiles.Count(t => t.TileType == _tile.TileType) == 3 && board.Wall.RemainingDraws > 0;
    }

    private bool CanPon(Board board, int seatIndex)
    {
      var seat = board.Seats[seatIndex];
      return !seat.DeclaredRiichi && seat.ConcealedTiles.Count(t => t.TileType == _tile.TileType) >= 2 && board.Wall.RemainingDraws > 0;
    }

    private bool CanChii(Board board, int seatIndex)
    {
      if ((board.ActiveSeatIndex + 1) % 4 != seatIndex)
      {
        return false;
      }

      var seat = board.Seats[seatIndex];
      if (seat.DeclaredRiichi || _tile.TileType.Suit == Suit.Jihai)
      {
        return false;
      }

      //var hasHonors = _state.ConcealedTiles.Any(t => t.TileType.Suit == Suit.Jihai);
      var tileType = _tile.TileType;
      var tileTypePresence = 0;
      foreach (var concealedTile in seat.ConcealedTiles.Where(t => t.TileType.Suit != Suit.Jihai))
      {
        tileTypePresence |= 1 << concealedTile.TileType.TileTypeId;
      }

      if (tileType.Index > 0 &&
          tileType.Index < 8 &&
          ((tileTypePresence >> (tileType.TileTypeId - 1)) & 0b101) == 0b101 &&
          seat.ConcealedTiles.Any(t => t.TileType != tileType)) // kuikae
      {
        return true;
      }

      if (tileType.Index < 7 &&
          ((tileTypePresence >> tileType.TileTypeId) & 0b110) == 0b110 &&
          seat.ConcealedTiles.Any(t => Kuikae.IsValidDiscardForNonKanchanChii(tileType, t.TileType)))
      {
        return true;
      }

      if (tileType.Index > 1 &&
          ((tileTypePresence >> (tileType.TileTypeId - 2)) & 0b011) == 0b011 &&
          seat.ConcealedTiles.Any(t => Kuikae.IsValidDiscardForNonKanchanChii(tileType, t.TileType)))
      {
        return true;
      }

      return false;
    }

    private bool CanRon(Board board, int seatIndex)
    {
      return AgariValidation.CanRon(board, seatIndex);
    }

    private class Client : IClient
    {
      public Client(int seatIndex, DiscardActions allowedActions)
      {
        _allowedActions = allowedActions;
        SeatIndex = seatIndex;
      }

      public bool Chii { get; private set; }

      public bool Daiminkan { get; private set; }

      public Tile? Discard { get; private set; }

      public bool IgnoredRon => _allowedActions.HasFlag(DiscardActions.Ron) && !Ron;

      public bool Pon { get; private set; }

      public bool Ron { get; private set; }

      public int SeatIndex { get; }

      public Tile? Tile0 { get; private set; }

      public Tile? Tile1 { get; private set; }

      void IClient.Discard(Tile tile)
      {
        throw new InvalidOperationException();
      }

      void IClient.Ankan(TileType tileType)
      {
        throw new InvalidOperationException();
      }

      void IClient.Shouminkan(Tile tile)
      {
        throw new InvalidOperationException();
      }

      void IClient.Tsumo()
      {
        throw new InvalidOperationException();
      }

      void IClient.Riichi(Tile tile)
      {
        throw new InvalidOperationException();
      }

      void IClient.KyuushuKyuuhai()
      {
        throw new InvalidOperationException();
      }

      void IClient.Pass()
      {
      }

      void IClient.Daiminkan()
      {
        Daiminkan = true;
      }

      void IClient.Pon(Tile tile0, Tile tile1, Tile discardAfterCall)
      {
        Pon = true;
        Tile0 = tile0;
        Tile1 = tile1;
        Discard = discardAfterCall;
      }

      void IClient.Chii(Tile tile0, Tile tile1, Tile discardAfterCall)
      {
        Chii = true;
        Tile0 = tile0;
        Tile1 = tile1;
        Discard = discardAfterCall;
      }

      void IClient.Ron()
      {
        Ron = true;
      }

      private readonly DiscardActions _allowedActions;
    }
  }
}