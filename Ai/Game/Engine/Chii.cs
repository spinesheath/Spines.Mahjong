using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Chii : State
  {
    private readonly int _seatIndex;
    private readonly Tile _tile0;
    private readonly Tile _tile1;
    private readonly Tile _discardAfterCall;

    public Chii(int seatIndex, Tile tile0, Tile tile1, Tile discardAfterCall)
    {
      _seatIndex = seatIndex;
      _tile0 = tile0;
      _tile1 = tile1;
      _discardAfterCall = discardAfterCall;
    }

    public override void Update(Board board, Wall wall)
    {
      var calledTile = board.CurrentDiscard!;
      ClearCurrentDiscard(board);
      board.ActiveSeatIndex = _seatIndex;
      board.ActiveSeat.Chii(calledTile, _tile0, _tile1);
    }

    public override Task Decide(Board board, Decider decider)
    {
      return Task.CompletedTask;
    }

    public override State Advance()
    {
      return new Discard(_discardAfterCall);
    }
  }
}