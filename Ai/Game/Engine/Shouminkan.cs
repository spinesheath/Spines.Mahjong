﻿using System.Linq;
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
      return _nextState!;
    }

    public override Task Decide(Board board, Decider decider)
    {
      var kans = board.Seats.SelectMany(s => s.Melds.Where(m => m.MeldType == MeldType.AddedKan || m.MeldType == MeldType.CalledKan || m.MeldType == MeldType.ClosedKan));
      if (kans.Count() == 4)
      {
        // TODO 4 kan abort
      }

      _nextState = new DoraIndicator(new AnkanDraw());

      return Task.CompletedTask;
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