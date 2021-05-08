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
      board.ClearCurrentDiscard();
      board.ActiveSeatIndex = _seatIndex;
      board.ActiveSeat.Daiminkan(calledTile);
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