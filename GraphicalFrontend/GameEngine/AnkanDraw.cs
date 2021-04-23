using System;
using GraphicalFrontend.Client;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.GameEngine
{
  internal class AnkanDraw : DrawBase
  {
    public override void Update(Board board)
    {
      var seat = board.ActiveSeat;

      var tile = board.Wall.DrawFromDeadWall();
      seat.Hand.Draw(tile.TileType);
      seat.ConcealedTiles.Add(tile);
      seat.CurrentDraw = tile;
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
      throw new InvalidOperationException();
    }

    protected override DrawActions GetPossibleActions(Board board)
    {
      var suggestedActions = DrawActions.Discard;
      suggestedActions |= CanTsumo(board) ? DrawActions.Tsumo : DrawActions.Discard;
      suggestedActions |= CanRiichi(board) ? DrawActions.Riichi : DrawActions.Discard;
      suggestedActions |= CanKan(board) ? DrawActions.Kan : DrawActions.Discard;
      return suggestedActions;
    }

    private bool CanTsumo(Board board)
    {
      // TODO rinshan info
      return AgariValidation.CanTsumo(board);
    }
  }
}