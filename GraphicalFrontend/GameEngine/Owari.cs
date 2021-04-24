using System.Linq;

namespace GraphicalFrontend.GameEngine
{
  internal class Owari : State
  {
    public override bool IsFinal { get; } = true;

    public override State Advance()
    {
      return new EndMatch();
    }

    public override void Update(Board board)
    {
      var highestScore = board.Seats.Max(s => s.Score);
      var firstPlace = board.Seats.First(s => s.Score == highestScore);
      firstPlace.Score += board.RiichiSticks * 1000;
    }
  }
}