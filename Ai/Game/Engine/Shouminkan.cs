using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;
using Meld = Game.Shared.Meld;

namespace Game.Engine
{
  internal class Shouminkan : State
  {
    private readonly Tile _tile;
    private State? _nextState;

    public Shouminkan(Tile tile)
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

    private readonly List<int> _ignoredRonSeats = new();

    public override async Task Decide(Board board, Decider decider)
    {
      var canChankan = new bool[4];

      var reactionTasks = new Task<bool>[4];
      for (var i = 0; i < 4; i++)
      {
        if (i == board.ActiveSeatIndex)
        {
          reactionTasks[i] = Task.FromResult(false);
          continue;
        }

        if (AgariValidation.CanChankan(board, i, _tile))
        {
          canChankan[i] = true;
          reactionTasks[i] = decider.OnChankan(i, _tile);
          continue;
        }
        
        reactionTasks[i] = Task.FromResult(false);
      }

      await Task.WhenAll(reactionTasks);

      for (var i = 0; i < 4; i++)
      {
        if (canChankan[i])
        {
          _ignoredRonSeats.Add(i);
        }
      }
      
      var ronCount = reactionTasks.Count(t => t.Result);
      if (ronCount == 3)
      {
        _nextState = new Abort();
        return;
      }

      if (ronCount > 0)
      {
        _nextState = new Ron(reactionTasks.Select((t, i) => new {t.Result, i}).Where(p => p.Result).Select(r => r.i));
        return;
      }

      if (board.Seats.SelectMany(s => s.Melds.Where(m => m.IsKan)).Count() == 4)
      {
        // TODO 4 kan abort if not by same player
      }

      _nextState = new DoraIndicator(new AnkanDraw());
    }

    public override void Update(Board board, Wall wall)
    {
      var seat = board.ActiveSeat;

      // kokushi chankan from ankan not allowed on tenhou
      // TODO 4 kan abort for all kans: if all 4 kan by the same player, no more kans are possible. If 4 kans by multiple players, game ends after the discard, unless ronned

      seat.CurrentDraw = null;
      seat.Hand.Ankan(_tile.TileType);
      seat.ConcealedTiles.RemoveAll(t => t.TileType == _tile.TileType);

      for (var i = 0; i < seat.Melds.Count; i++)
      {
        var meld = seat.Melds[i];
        if (meld.MeldType == MeldType.Koutsu && meld.LowestTile.TileType == _tile.TileType)
        {
          seat.Melds[i] = Meld.Shouminkan(meld.CalledTile!, _tile);
          break;
        }
      }
    }
  }
}