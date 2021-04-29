using System.Linq;
using Game.Shared;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Owari : State
  {
    public override bool IsFinal { get; } = true;

    public override State Advance()
    {
      return new EndMatch();
    }

    public override void Update(Board board, Wall wall)
    {
      var highestScore = board.Seats.Max(s => s.Score);
      var firstPlace = board.Seats.First(s => s.Score == highestScore);
      firstPlace.Score += board.RiichiSticks * 1000;
    }
  }
}