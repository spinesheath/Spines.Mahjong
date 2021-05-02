using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Shanten;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class InitGame : State
  {
    public override State Advance()
    {
      Debug.Assert(_nextState != null, "call Decide() before Advance()");
      return _nextState;
    }

    public override Task Decide(Board board, Decider decider)
    {
      for (var i = 0; i < 4; i++)
      {
        if (board.Seats[i].SeatWind == TileType.Ton)
        {
          _nextState = new Draw(i);
        }
      }

      return Task.CompletedTask;
    }

    public override void Update(Board board, Wall wall)
    {
      wall.Shuffle();
      wall.RevealDoraIndicator();

      for (var i = 0; i < 4; i++)
      {
        var hand = wall.DrawInitialHand();
        board.Seats[i].Init(hand);
      }
    }

    private State? _nextState;
  }
}