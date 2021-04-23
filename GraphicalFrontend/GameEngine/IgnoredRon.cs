using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphicalFrontend.GameEngine
{
  internal class IgnoredRon : State
  {
    public IgnoredRon(IEnumerable<int> seatIndexes, State nextState)
    {
      _nextState = nextState;
      _seatIndexes = seatIndexes.ToList();
    }

    public override State Advance()
    {
      return _nextState;
    }

    public override Task Decide(Board board, Decider decider)
    {
      return Task.CompletedTask;
    }

    public override void Update(Board board)
    {
      foreach (var seat in _seatIndexes)
      {
        board.Seats[seat].IgnoredRonFuriten = true;
      }
    }

    private readonly State _nextState;
    private readonly IReadOnlyList<int> _seatIndexes;
  }
}