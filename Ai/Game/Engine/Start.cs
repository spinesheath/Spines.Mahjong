using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis;

namespace Game.Engine
{
  internal class Start : State
  {
    public override State Advance()
    {
      return new InitGame();
    }

    public override Task Decide(Board board, Decider decider)
    {
      return Task.CompletedTask;
    }

    public override void Update(Board board, Wall wall)
    {
      for (var i = 0; i < 4; i++)
      {
        var seat = board.Seats[i];
        seat.SeatWind = TileType.FromSuitAndIndex(Suit.Jihai, i);
        seat.Score = 25000;
      }
    }
  }
}