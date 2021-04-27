using System;
using Game.Shared;
using Spines.Mahjong.Analysis;

namespace Game.Engine
{
  internal class ShouminkanDraw : DrawBase
  {
    public override void Ankan(TileType tileType)
    {
      NextState = new DoraIndicator(new Ankan(tileType));
    }

    public override void Discard(Tile tile)
    {
      NextState = new DoraIndicator(new Discard(tile));
    }

    public override void KyuushuKyuuhai()
    {
      throw new InvalidOperationException();
    }

    public override void Riichi(Tile tile)
    {
      throw new InvalidOperationException();
    }

    public override void Shouminkan(Tile tile)
    {
      NextState = new DoraIndicator(new Shouminkan(tile));
    }

    public override void Update(Board board, Wall wall)
    {
      var seat = board.ActiveSeat;

      var tile = wall.DrawFromDeadWall();
      seat.Hand.Draw(tile.TileType);
      seat.ConcealedTiles.Add(tile);
      seat.CurrentDraw = tile;
    }

    protected override DrawActions GetPossibleActions(Board board)
    {
      var suggestedActions = DrawActions.Discard;
      suggestedActions |= CanTsumo(board) ? DrawActions.Tsumo : DrawActions.Discard;
      suggestedActions |= CanKan(board) ? DrawActions.Kan : DrawActions.Discard;
      return suggestedActions;
    }

    private static bool CanTsumo(Board board)
    {
      return AgariValidation.CanTsumo(board, true);
    }
  }
}