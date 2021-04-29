using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Riichi : State
  {
    private readonly Tile _tile;

    public Riichi(Tile tile)
    {
      _tile = tile;
    }

    public override State Advance()
    {
      return new RiichiDiscard(_tile);
    }

    public override void Update(Board board, Wall wall)
    {
      board.ActiveSeat.DeclaredRiichi = true;
    }
  }
}