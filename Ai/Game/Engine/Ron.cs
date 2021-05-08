using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Ron : State
  {
    private readonly IReadOnlyList<int> _seatIndexes;
    private State? _nextState;

    public Ron(IEnumerable<int> seatIndexes)
    {
      _seatIndexes = seatIndexes.OrderBy(x => x).ToList();
    }

    public override void Update(Board board, Wall wall)
    {
    }

    public override Task Decide(Board board, Decider decider)
    {
      var boardPointsToIndex = _seatIndexes.SkipWhile(i => i < board.ActiveSeatIndex).DefaultIfEmpty(_seatIndexes[0]).First();

      _nextState = new EndGame(_seatIndexes);
      foreach (var seatIndex in _seatIndexes)
      {
        // TODO calculate ron score (include honba and riichi sticks)
        
        var scoreChanges = new int[4];
        var getsBoardPoints = seatIndex == boardPointsToIndex;
        var honbaPoints = getsBoardPoints ? board.Honba * 300 : 0;
        var riichiPoints = getsBoardPoints ? board.RiichiSticks * 1000 : 0;
        scoreChanges[seatIndex] = 4000 + riichiPoints + honbaPoints;
        scoreChanges[board.ActiveSeatIndex] = -4000 - honbaPoints;

        _nextState = new Payment(_nextState, new PaymentInformation(0, 0, scoreChanges, Yaku.None));
      }

      return Task.CompletedTask;
    }

    public override State Advance()
    {
      return _nextState!;
    }
  }
}