using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Ankan : State
  {
    private readonly TileType _tileType;

    public Ankan(TileType tileType)
    {
      _tileType = tileType;
    }

    public override State Advance()
    {
      return new DoraIndicator(new AnkanDraw());
    }

    public override void Update(Board board, Wall wall)
    {
      board.ActiveSeat.Ankan(_tileType);
    }
  }
}