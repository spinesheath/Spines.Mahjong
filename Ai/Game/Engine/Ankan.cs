using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.State;
using Meld = Spines.Mahjong.Analysis.State.Meld;

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
      var seat = board.ActiveSeat;

      seat.CurrentDraw = null;
      seat.Hand.Ankan(_tileType);
      seat.ConcealedTiles.RemoveAll(t => t.TileType == _tileType);
      seat.Melds.Add(Meld.Ankan(_tileType));
    }
  }
}