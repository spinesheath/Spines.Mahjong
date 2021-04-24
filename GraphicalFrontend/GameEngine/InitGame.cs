using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Shanten;

namespace GraphicalFrontend.GameEngine
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

    public override void Update(Board board)
    {
      board.Wall.Shuffle();
      board.Wall.RevealDoraIndicator();

      for (var i = 0; i < 4; i++)
      {
        var seat = board.Seats[i];
        seat.DeclaredRiichi = false;
        seat.CurrentDiscard = null;
        seat.CurrentDraw = null;
        seat.ConcealedTiles.Clear();
        seat.Melds.Clear();
        seat.Discards.Clear();
        seat.Hand = new UkeIreCalculator();
        
        var hand = board.Wall.DrawInitialHand().ToList();
        seat.ConcealedTiles.AddRange(hand);
        seat.Hand.Init(hand.Select(t => t.TileType));
      }
    }

    private State? _nextState;
  }
}