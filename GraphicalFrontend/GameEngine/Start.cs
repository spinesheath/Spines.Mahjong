using System.Threading.Tasks;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.GameEngine
{
  internal class Start : State
  {
    public override void Update(Board board)
    {
      for (var i = 0; i < 4; i++)
      {
        var seat = board.Seats[i];
        seat.SeatWind = TileType.FromSuitAndIndex(Suit.Jihai, i);
        seat.Score = 25000;
      }
    }

    public override Task Decide(Board board, Decider decider)
    {
      return Task.CompletedTask;
    }

    public override State Advance()
    {
      return new InitGame();
    }
  }
}