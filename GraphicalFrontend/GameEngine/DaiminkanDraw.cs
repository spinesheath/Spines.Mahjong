using System;
using GraphicalFrontend.Client;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.GameEngine
{
  internal class DaiminkanDraw : DrawBase
  {
    public override void Update(Board board)
    {
      var seat = board.ActiveSeat;

      var tile = board.Wall.DrawFromDeadWall();
      seat.Hand.Draw(tile.TileType);
      seat.ConcealedTiles.Add(tile);
      seat.CurrentDraw = tile;
      seat.IgnoredRonFuriten = false;
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
      // TODO rinshan info
      return AgariValidation.CanTsumo(board);
    }

    public override void Discard(Tile tile)
    {
      // TODO exact timing of dora indicator, like can opponents see dora when deciding on call? 
      // TODO document in board that there is a pending dora flip - relevant information for calling a tile
      NextState = new DoraIndicator(new Discard(tile));
    }

    public override void Ankan(TileType tileType)
    {
      NextState = new DoraIndicator(new Ankan(tileType));
    }

    public override void Shouminkan(Tile tile)
    {
      NextState = new DoraIndicator(new Shouminkan(tile));
    }

    public override void Riichi(Tile tile)
    {
      throw new InvalidOperationException();
    }

    public override void KyuushuKyuuhai()
    {
      throw new InvalidOperationException();
    }
  }
}