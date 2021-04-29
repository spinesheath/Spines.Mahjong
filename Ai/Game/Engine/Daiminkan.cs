using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Daiminkan : State
  {
    private readonly int _seatIndex;

    public Daiminkan(int seatIndex)
    {
      _seatIndex = seatIndex;
    }

    public override void Update(Board board, Wall wall)
    {
      var calledTile = board.CurrentDiscard!;
      
      ClearCurrentDiscard(board);
      
      board.ActiveSeatIndex = _seatIndex;
      var seat = board.ActiveSeat;
      seat.Hand.Daiminkan(calledTile.TileType);
      seat.ConcealedTiles.RemoveAll(t => t.TileType == calledTile.TileType);
      seat.Melds.Add(Meld.Daiminkan(calledTile));
    }

    public override Task Decide(Board board, Decider decider)
    {
      return Task.CompletedTask;
    }

    public override State Advance()
    {
      return new DaiminkanDraw();
    }
  }
}