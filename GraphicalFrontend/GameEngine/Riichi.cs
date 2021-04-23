using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.GameEngine
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

    public override void Update(Board board)
    {
      board.ActiveSeat.DeclaredRiichi = true;
    }
  }
}