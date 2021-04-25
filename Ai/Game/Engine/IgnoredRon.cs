using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Shared;

namespace Game.Engine
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

    public override void Update(Board board, Wall wall)
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