using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.State;

namespace Spines.Mahjong.Analysis.Score
{
  public static class ScoreCalculator
  {
    public static PaymentInformation Tsumo(Board board, bool isRinshanDraw)
    {
      var seat = board.ActiveSeat;
      Debug.Assert(seat.Hand.Shanten == -1);

      var yaku = Yaku.None;
      var yakuman = Yaku.None;
      var han = 0;
      var yakumanCount = 0;

      // kokushi
      if (seat.Hand.KokushiShanten == -1)
      {
        yakuman |= Yaku.KokushiMusou;
        yakumanCount += 1;
      }

      // chiitoitsu
      if (seat.Hand.ChiitoitsuShanten == -1)
      {
        yaku |= Yaku.Chiitoitsu;
        han += 2;
      }

      var isOpenHand = seat.Melds.Any(m => m.MeldType != MeldType.ClosedKan);

      // tsumo
      if (!isOpenHand)
      {
        yaku |= Yaku.MenzenTsumo;
        han += 1;
      }

      // TODO double riichi, ippatsu
      if (seat.DeclaredRiichi)
      {
        yaku |= Yaku.Riichi;
        han += 1;
      }

      // presence of tile types as bit flag. 1m is least significant bit.
      // TODO calculate tanyao possibility together with shape based yaku?
      var tileTypePresences = 0L;
      foreach (var tile in seat.ConcealedTiles.Concat(seat.Melds.SelectMany(m => m.Tiles)))
      {
        tileTypePresences |= 1L << tile.TileType.TileTypeId;
      }

      // tanyao
      const long tanyaohaiBits = 0b0000000_011111110_011111110_011111110L;
      if ((tileTypePresences & tanyaohaiBits) == tileTypePresences)
      {
        yaku |= isOpenHand ? Yaku.OpenTanyao : Yaku.ClosedTanyao;
        han += 1;
      }

      // number of tiles per tileType in the concealed part of the hand
      // TODO calculate this together with shape based yaku?
      var tileTypeCounts = new int[34];
      var tileCountsBySuit = new int[4];
      foreach (var tile in seat.ConcealedTiles)
      {
        tileTypeCounts[tile.TileType.TileTypeId] += 1;
        tileCountsBySuit[tile.TileType.SuitId] += 1;
      }

      // how often each number occurs in tileTypeCounts
      var countOfTileTypeCounts = new int[5];
      foreach (var count in tileTypeCounts)
      {
        countOfTileTypeCounts[count] += 1;
      }

      // toitoi
      // honroutou always fulfills either chiitoitsu or toitoi
      // suuankou and chinroutou always fulfill toitoi
      //if (countOfTileTypeCounts[2] == 1 && countOfTileTypeCounts[0] + countOfTileTypeCounts[1] + countOfTileTypeCounts[4] == 0 && seat.Melds.All(m => m.MeldType != MeldType.Shuntsu))
      //{
      //  return true;
      //}

      // suukantsu
      var kanCount = seat.Melds.Count(m => m.MeldType == MeldType.AddedKan || m.MeldType == MeldType.CalledKan || m.MeldType == MeldType.ClosedKan);
      if (kanCount == 4)
      {
        yakuman |= Yaku.Suukantsu;
        yakumanCount += 1;
      }

      // sankantsu
      if (kanCount == 3)
      {
        yaku |= Yaku.Sankantsu;
        han += 2;
      }

      // Ryuuiisou
      const long ryuuiisouBits = 0b0100000_010101110_000000000_000000000L;
      if ((tileTypePresences & ryuuiisouBits) == tileTypePresences)
      {
        yakuman |= Yaku.Ryuuiisou;
        yakumanCount += 1;
      }

      // Tsuuiisou
      const long tsuuiisouBits = 0b1111111_000000000_000000000_000000000L;
      if ((tileTypePresences & tsuuiisouBits) == tileTypePresences)
      {
        yakuman |= Yaku.Tsuuiisou;
        yakumanCount += 1;
      }

      // TODO chuuren: per suit 1 if either empty suit or chuuren, 0 else
      // honitsu
      // chinitsu, shousuushii, daisuushii and chuuren poutou always fulfill honitsu
      //if (tileCountsBySuit.Take(3).Count(t => t == 0) == 2)
      //{
      //  return true;
      //}

      // yakuhai
      // TODO shousangen and daisangen
      var yakuhaiTileTypeIds = new List<int> { board.RoundWind.TileTypeId, seat.SeatWind.TileTypeId, 31, 32, 33 };
      //if (seat.Melds.Any(m => yakuhaiTileTypeIds.Contains(m.LowestTile.TileType.TileTypeId)) || yakuhaiTileTypeIds.Any(i => tileTypeCounts[i] == 3))
      //{
      //  return true;
      //}

      if (yakumanCount > 0)
      {
        // TODO pao
        var scoreChanges = new int[4];
        // TODO method for doing score changes easily
        scoreChanges[board.ActiveSeatIndex] = yakumanCount * (seat.IsOya ? 48000 : 32000);
        return new PaymentInformation(0, 0, scoreChanges, yakuman);
      }

      return new PaymentInformation(0, han, new int[4], yaku);
    }
  }
}