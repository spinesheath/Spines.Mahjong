using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.GameEngine
{
  internal class EndGame : State
  {
    public EndGame(IEnumerable<int> winningSeatIndexes)
    {
      _winningSeatIndexes = winningSeatIndexes.ToList();
    }

    public override State Advance()
    {
      return _nextState!;
    }

    public override void Update(Board board)
    {
      if (board.Seats.Any(s => s.Score < 0))
      {
        _nextState = new Owari();
        return;
      }

      var oorasu = board.RoundWind == TileType.Nan && board.Seats[3].IsOya;
      if ((oorasu || board.RoundWind == TileType.Shaa) && board.Seats.Any(s => s.Score >= 30000))
      {
        _nextState = new Owari();
        return;
      }

      var oyaWin = _winningSeatIndexes.Any(i => board.Seats[i].IsOya);
      var oyaTenpai = board.Oya.Hand.Shanten == 0;
      var oyaHighestScore = board.Oya.Score > board.Seats.Where(s => !s.IsOya).Max(s => s.Score);

      if (oyaWin || oyaTenpai)
      {
        if (oorasu && oyaHighestScore && board.Oya.Score >= 30000)
        {
          _nextState = new Owari();
          return;
        }
        else
        {
          board.Honba += 1;
          _nextState = new InitGame();
          return;
        }
      }

      if (!_winningSeatIndexes.Any())
      {
        RotateWinds(board);
      }
      else
      {
        board.Honba = 0;
        RotateWinds(board);
      }

      if (board.RoundWind == TileType.Pei)
      {
        _nextState = new Owari();
        return;
      }

      _nextState = new InitGame();
    }

    private readonly IEnumerable<int> _winningSeatIndexes;

    private State? _nextState;

    private static void RotateWinds(Board board)
    {
      var wind0 = board.Seats[0].SeatWind;
      board.Seats[0].SeatWind = board.Seats[3].SeatWind;
      board.Seats[3].SeatWind = board.Seats[2].SeatWind;
      board.Seats[2].SeatWind = board.Seats[1].SeatWind;
      board.Seats[1].SeatWind = wind0;

      if (board.Seats[0].SeatWind == TileType.Ton)
      {
        board.RoundWind = TileType.FromTileTypeId(board.RoundWind.TileTypeId + 1);
      }
    }
  }
}