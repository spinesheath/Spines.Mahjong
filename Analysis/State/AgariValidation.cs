using System.Linq;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.State
{
  public static class AgariValidation
  {
    public static bool CanTsumo(Board board, bool isRinshanDraw)
    {
      var seat = board.ActiveSeat;
      if (seat.Hand.Shanten != -1)
      {
        return false;
      }

      // Most yaku
      var w = new WindScoringData(board.RoundWind.Index, seat.SeatWind.Index);
      if (seat.Hand.ScoringData.Flags(w, seat.CurrentDraw!.TileType, false).Item1 != 0)
      {
        return true;
      }

      // riichi, double riichi, ippatsu
      if (seat.DeclaredRiichi)
      {
        return true;
      }

      // tenhou
      if (board.IsFirstGoAround)
      {
        return true;
      }

      // haitei raoyue
      if (board.Wall.RemainingDraws == 0)
      {
        return true;
      }

      // rinshan kaihou
      if (seat.CurrentDraw != null && isRinshanDraw && board.Wall.RemainingDraws > 0)
      {
        return true;
      }

      return false;
    }

    public static bool CanRon(Board board, int seatIndex)
    {
      var seat = board.Seats[seatIndex];

      if (seat.IgnoredRonFuriten)
      {
        return false;
      }

      var winningTile = board.Seats[board.ActiveSeatIndex].CurrentDiscard!;
      var hand = (HandCalculator)seat.Hand.WithTile(winningTile.TileType);

      if (hand.Shanten != -1)
      {
        return false;
      }

      var furitenTileTypes = seat.Hand.GetFuritenTileTypes().ToList();
      if (furitenTileTypes.Intersect(seat.Discards.Select(t => t.TileType)).Any())
      {
        return false;
      }

      // Most yaku
      var w = new WindScoringData(board.RoundWind.Index, seat.SeatWind.Index);
      if (hand.ScoringData.Flags(w, winningTile.TileType, true).Item1 != 0)
      {
        return true;
      }

      // riichi, double riichi, ippatsu
      if (seat.DeclaredRiichi)
      {
        return true;
      }

      // chiihou
      if (board.IsFirstGoAround)
      {
        return true;
      }

      // houtei raoyui
      if (board.Wall.RemainingDraws == 0)
      {
        return true;
      }
      
      return false;
    }

    public static bool CanChankan(Board board, int seatIndex, Tile addedTile)
    {
      return board.Seats[seatIndex].Hand.WithTile(addedTile.TileType).Shanten == -1;
    }
  }
}