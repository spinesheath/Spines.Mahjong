using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.GameEngine
{
  // TODO check if some helper states can be moved into an Update method of the preceding state
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
      // TODO what happens on Owari with unclaimed riichi sticks? -> go to first place

      if (board.Seats.Any(s => s.Score < 0))
      {
        _nextState = new Owari();
        return;
      }

      if (board.RoundWind == TileType.Shaa && board.Seats.Any(s => s.Score >= 30000))
      {
        _nextState = new Owari();
        return;
      }

      var oyaWin = _winningSeatIndexes.Any(i => board.Seats[i].IsOya);
      var oyaTenpai = board.Oya.Hand.Shanten == 0;
      var oyaHighestScore = board.Oya.Score > board.Seats.Where(s => !s.IsOya).Max(s => s.Score);
      if ((oyaWin || oyaTenpai) && oyaHighestScore)
      {
        _nextState = new Owari();
        return;
      }

      if (oyaWin || oyaTenpai)
      {
        board.Honba += 1;
        _nextState = new InitGame();
        return;
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