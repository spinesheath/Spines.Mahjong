﻿using System.Linq;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Draw : DrawBase
  {
    public Draw(int seatIndex)
    {
      _seatIndex = seatIndex;
    }

    public override void Update(Board board, Wall wall)
    {
      board.ClearCurrentDiscard();
      board.ActiveSeatIndex = _seatIndex;
      var seat = board.ActiveSeat;

      seat.Draw(wall.Draw());
      if (!seat.DeclaredRiichi)
      {
        seat.IgnoredRonFuriten = false;
      }
    }

    public override void Discard(Tile tile)
    {
      NextState = new Discard(tile);
    }

    public override void Ankan(TileType tileType)
    {
      NextState = new Ankan(tileType);
    }

    public override void Shouminkan(Tile tile)
    {
      NextState = new Shouminkan(tile);
    }

    public override void Riichi(Tile tile)
    {
      NextState = new Riichi(tile);
    }

    public override void KyuushuKyuuhai()
    {
      NextState = new Abort();
    }

    private readonly int _seatIndex;

    protected override DrawActions GetPossibleActions(Board board)
    {
      var suggestedActions = DrawActions.Discard;
      suggestedActions |= CanTsumo(board) ? DrawActions.Tsumo : DrawActions.Discard;
      suggestedActions |= CanRiichi(board) ? DrawActions.Riichi : DrawActions.Discard;
      suggestedActions |= CanKan(board) ? DrawActions.Kan : DrawActions.Discard;
      suggestedActions |= CanKyuushuKyuuhai(board) ? DrawActions.KyuushuKyuuhai : DrawActions.Discard;
      return suggestedActions;
    }

    private static bool CanTsumo(Board board)
    {
      return AgariValidation.CanTsumo(board, false);
    }

    private static bool CanKyuushuKyuuhai(Board board)
    {
      var isFirstGoAround = board.Wall.RemainingDraws >= 66 && board.Seats.All(s => s.Melds.Count == 0);
      return isFirstGoAround && board.ActiveSeat.ConcealedTiles.GroupBy(t => t.TileType).Count(g => g.Key.IsKyuuhai) > 8;
    }
  }
}