using System;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class AnkanDraw : DrawBase
  {
    public override void Ankan(TileType tileType)
    {
      NextState = new Ankan(tileType);
    }

    public override void Discard(Tile tile)
    {
      NextState = new Discard(tile);
    }

    public override void KyuushuKyuuhai()
    {
      throw new InvalidOperationException();
    }

    public override void Riichi(Tile tile)
    {
      NextState = new Riichi(tile);
    }

    public override void Shouminkan(Tile tile)
    {
      NextState = new Shouminkan(tile);
    }

    public override void Update(Board board, Wall wall)
    {
      board.ActiveSeat.Draw(wall.DrawFromDeadWall());
    }

    protected override DrawActions GetPossibleActions(Board board)
    {
      var suggestedActions = DrawActions.Discard;
      suggestedActions |= CanTsumo(board) ? DrawActions.Tsumo : DrawActions.Discard;
      suggestedActions |= CanRiichi(board) ? DrawActions.Riichi : DrawActions.Discard;
      suggestedActions |= CanKan(board) ? DrawActions.Kan : DrawActions.Discard;
      return suggestedActions;
    }

    private static bool CanTsumo(Board board)
    {
      return AgariValidation.CanTsumo(board, true);
    }
  }
}