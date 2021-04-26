using System;
using System.Collections.Generic;
using System.Linq;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;

namespace Game.Engine
{
  internal static class AgariValidation
  {
    // TODO consolidate methods

    public static bool CanTsumo(Board board, bool isRinshanDraw)
    {
      var seat = board.ActiveSeat;
      if (seat.Hand.Shanten != -1)
      {
        return false;
      }

      // kokushi
      if (seat.Hand.KokushiShanten == -1)
      {
        return true;
      }

      // chiitoitsu
      // honroutou always fulfills either chiitoitsu or toitoi
      if (seat.Hand.ChiitoitsuShanten == -1)
      {
        return true;
      }

      // riichi
      // ippatsu and double riichi always fulfill riichi
      if (seat.DeclaredRiichi)
      {
        return true;
      }

      var isOpenHand = seat.Melds.Any(m => m.MeldType != MeldType.ClosedKan);

      // tsumo
      if (!isOpenHand)
      {
        return true;
      }

      // presence of tile types as bit flag. 1m is least significant bit.
      var tileTypePresences = 0L;
      foreach (var tile in seat.ConcealedTiles.Concat(seat.Melds.SelectMany(m => m.Tiles)))
      {
        tileTypePresences |= 1L << tile.TileType.TileTypeId;
      }
      
      // tanyao
      const long tanyaohaiBits = 0b0000000_011111110_011111110_011111110L;
      if ((tileTypePresences & tanyaohaiBits) == tileTypePresences)
      {
        return true;
      }

      // number of tiles per tileType in the concealed part of the hand
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
      if (countOfTileTypeCounts[2] == 1 && countOfTileTypeCounts[0] + countOfTileTypeCounts[1] + countOfTileTypeCounts[4] == 0 && seat.Melds.All(m => m.MeldType != MeldType.Shuntsu))
      {
        return true;
      }

      // sankantsu
      // suukantsu always fulfills sankantsu
      if (seat.Melds.Count(m => m.MeldType == MeldType.AddedKan || m.MeldType == MeldType.CalledKan || m.MeldType == MeldType.ClosedKan) >= 3)
      {
        return true;
      }

      // honitsu
      // chinitsu, shousuushii, daisuushii, tsuuiisou, ryuuiisou and chuuren poutou always fulfill honitsu
      if (tileCountsBySuit.Take(3).Count(t => t == 0) == 2)
      {
        return true;
      }

      // yakuhai
      // shousangen and daisangen always fulfill yakuhai
      var yakuhaiTileTypeIds = new List<int> { board.RoundWind.TileTypeId, seat.SeatWind.TileTypeId, 31, 32, 33 };
      if (seat.Melds.Any(m => yakuhaiTileTypeIds.Contains(m.LowestTile.TileType.TileTypeId)) || yakuhaiTileTypeIds.Any(i => tileTypeCounts[i] == 3))
      {
        return true;
      }
      
      // 3 ittsuu
      // 1 junchan
      // 1 iipeikou
      // 1 pinfu if wait not in this suit
      // 9 pinfu if wait in this suit, indexed by wait
      // 7 shuntsu presence for sanshoku doujun
      // 9 koutsu presence for sanshoku doukou
      // 3 ankou count if wait not in this suit
      // 3*9 ankou count if wait in this suit, indexed by wait
      var manzuFlags = ShapeBasedYakuFlags.GetFlagsForSuit(new ArraySegment<int>(tileTypeCounts, 0, 9));
      var pinzuFlags = ShapeBasedYakuFlags.GetFlagsForSuit(new ArraySegment<int>(tileTypeCounts, 9, 9));
      var souzuFlags = ShapeBasedYakuFlags.GetFlagsForSuit(new ArraySegment<int>(tileTypeCounts, 18, 9));
      var flagsBySuit = new [] {manzuFlags, pinzuFlags, souzuFlags};
      var chantaMelds = true;

      foreach (var meld in seat.Melds)
      {
        var index = meld.LowestTile.TileType.Index;
        var suitId = meld.LowestTile.TileType.SuitId;
        if (suitId == 3)
        {
          continue;
        }

        if (meld.MeldType == MeldType.Shuntsu)
        {
          flagsBySuit[suitId] |= 1L << (index + 15);
          chantaMelds &= index == 0 || index == 6;
          if (index % 3 == 0)
          {
            flagsBySuit[suitId] |= 1L << (index / 3);
          }
        }
        else
        {
          flagsBySuit[suitId] |= 1L << (index + 24);
          chantaMelds &= index == 0 || index == 8;
        }
      }

      var orFlags = manzuFlags | pinzuFlags | souzuFlags;

      // ittsuu
      if ((orFlags & 0b111L) == 0b111L)
      {
        return true;
      }

      // iipeikou
      // ryanpeikou always fulfills iipeikou
      if ((orFlags & (1L << 4)) != 0 && !isOpenHand)
      {
        return true;
      }

      var andFlags = manzuFlags & pinzuFlags & souzuFlags;

      // pinfu
      var pinfuTileTypes = 0b000_0000_111111111_111111111_111111111L;
      pinfuTileTypes |= 1L << seat.SeatWind.TileTypeId;
      pinfuTileTypes |= 1L << board.RoundWind.TileTypeId;
      if (seat.Melds.Count == 0 && (tileTypePresences & pinfuTileTypes) == tileTypePresences && (andFlags & (1L << 5)) != 0)
      {
        if (seat.CurrentDraw!.TileType.SuitId == 3 || (flagsBySuit[seat.CurrentDraw.TileType.SuitId] & (1L << 6 + seat.CurrentDraw.TileType.Index)) != 0)
        {
          return true;
        }
      }

      // chanta
      // junchan always fulfills chanta
      if ((andFlags & (1L << 5)) != 0 && chantaMelds)
      {
        return true;
      }

      // sanshoku doujun
      // sanshoku doukou
      if ((andFlags & (0b111111111_1111111L << 15)) != 0)
      {
        return true;
      }

      // sanankou
      var ankouCount = seat.Melds.Count(m => m.MeldType == MeldType.ClosedKan) + tileTypeCounts.Skip(27).Count(c => c == 3);
      for (var i = 0; i < 3; i++)
      {
        ankouCount += (int)((flagsBySuit[i] >> 31) & 0b111);
      }

      if (ankouCount >= 3)
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
      var hand = seat.Hand.WithTile(winningTile.TileType);

      if (hand.Shanten != -1)
      {
        return false;
      }

      var furitenTileTypes = seat.Hand.GetFuritenTileTypes().ToList();
      if (furitenTileTypes.Intersect(seat.Discards.Select(t => t.TileType)).Any())
      {
        return false;
      }

      // kokushi
      if (hand.KokushiShanten == -1)
      {
        return true;
      }

      // chiitoitsu
      // honroutou always fulfills either chiitoitsu or toitoi
      if (hand.ChiitoitsuShanten == -1)
      {
        return true;
      }

      // riichi
      // ippatsu and double riichi always fulfill riichi
      if (seat.DeclaredRiichi)
      {
        return true;
      }

      // presence of tile types as bit flag. 1m is least significant bit.
      var tileTypePresences = 0L;
      foreach (var tile in seat.ConcealedTiles.Concat(seat.Melds.SelectMany(m => m.Tiles)))
      {
        tileTypePresences |= 1L << tile.TileType.TileTypeId;
      }

      tileTypePresences |= 1L << winningTile.TileType.TileTypeId;

      // tanyao
      const long tanyaohaiBits = 0b0000000_011111110_011111110_011111110L;
      if ((tileTypePresences & tanyaohaiBits) == tileTypePresences)
      {
        return true;
      }

      // number of tiles per tileType in the concealed part of the hand
      var tileTypeCounts = new int[34];
      var tileCountsBySuit = new int[4];
      foreach (var tile in seat.ConcealedTiles)
      {
        tileTypeCounts[tile.TileType.TileTypeId] += 1;
        tileCountsBySuit[tile.TileType.SuitId] += 1;
      }

      tileTypeCounts[winningTile.TileType.TileTypeId] += 1;
      tileCountsBySuit[winningTile.TileType.SuitId] += 1;

      // how often each number occurs in tileTypeCounts
      var countOfTileTypeCounts = new int[5];
      foreach (var count in tileTypeCounts)
      {
        countOfTileTypeCounts[count] += 1;
      }

      // toitoi
      // honroutou always fulfills either chiitoitsu or toitoi
      // suuankou and chinroutou always fulfill toitoi
      if (countOfTileTypeCounts[2] == 1 && countOfTileTypeCounts[0] + countOfTileTypeCounts[1] + countOfTileTypeCounts[4] == 0 && seat.Melds.All(m => m.MeldType != MeldType.Shuntsu))
      {
        return true;
      }

      // sankantsu
      // suukantsu always fulfills sankantsu
      if (seat.Melds.Count(m => m.MeldType == MeldType.AddedKan || m.MeldType == MeldType.CalledKan || m.MeldType == MeldType.ClosedKan) >= 3)
      {
        return true;
      }

      // honitsu
      // chinitsu, shousuushii, daisuushii, tsuuiisou, ryuuiisou and chuuren poutou always fulfill honitsu
      if (tileCountsBySuit.Take(3).Count(t => t == 0) == 2)
      {
        return true;
      }

      // yakuhai
      // shousangen and daisangen always fulfill yakuhai
      var yakuhaiTileTypeIds = new List<int> {board.RoundWind.TileTypeId, seat.SeatWind.TileTypeId, 31, 32, 33};
      if (seat.Melds.Any(m => yakuhaiTileTypeIds.Contains(m.LowestTile.TileType.TileTypeId)) || yakuhaiTileTypeIds.Any(i => tileTypeCounts[i] == 3))
      {
        return true;
      }

      // 3 ittsuu
      // 1 junchan
      // 1 iipeikou
      // 1 pinfu if wait not in this suit
      // 9 pinfu if wait in this suit, indexed by wait
      // 7 shuntsu presence for sanshoku doujun
      // 9 koutsu presence for sanshoku doukou
      // 3 ankou count if wait not in this suit
      // 3*9 ankou count if wait in this suit, indexed by wait
      var manzuFlags = ShapeBasedYakuFlags.GetFlagsForSuit(new ArraySegment<int>(tileTypeCounts, 0, 9));
      var pinzuFlags = ShapeBasedYakuFlags.GetFlagsForSuit(new ArraySegment<int>(tileTypeCounts, 9, 9));
      var souzuFlags = ShapeBasedYakuFlags.GetFlagsForSuit(new ArraySegment<int>(tileTypeCounts, 18, 9));
      var isOpenHand = seat.Melds.Any(m => m.MeldType != MeldType.ClosedKan);
      var flagsBySuit = new[] {manzuFlags, pinzuFlags, souzuFlags};
      var chantaMelds = true;

      foreach (var meld in seat.Melds)
      {
        var index = meld.LowestTile.TileType.Index;
        var suitId = meld.LowestTile.TileType.SuitId;
        if (suitId == 3)
        {
          continue;
        }

        if (meld.MeldType == MeldType.Shuntsu)
        {
          flagsBySuit[suitId] |= 1L << (index + 15);
          chantaMelds &= index == 0 || index == 6;
          if (index % 3 == 0)
          {
            flagsBySuit[suitId] |= 1L << (index / 3);
          }
        }
        else
        {
          flagsBySuit[suitId] |= 1L << (index + 24);
          chantaMelds &= index == 0 || index == 8;
        }
      }

      var orFlags = manzuFlags | pinzuFlags | souzuFlags;

      // ittsuu
      if ((orFlags & 0b111L) == 0b111L)
      {
        return true;
      }

      // iipeikou
      // ryanpeikou always fulfills iipeikou
      if ((orFlags & (1L << 4)) != 0 && !isOpenHand)
      {
        return true;
      }

      var andFlags = manzuFlags & pinzuFlags & souzuFlags;

      // pinfu
      var pinfuTileTypes = 0b000_0000_111111111_111111111_111111111L;
      pinfuTileTypes |= 1L << seat.SeatWind.TileTypeId;
      pinfuTileTypes |= 1L << board.RoundWind.TileTypeId;
      if (seat.Melds.Count == 0 && (tileTypePresences & pinfuTileTypes) == tileTypePresences && (andFlags & (1L << 5)) != 0)
      {
        if (winningTile.TileType.SuitId == 3 || (flagsBySuit[winningTile.TileType.SuitId] & (1L << 6 + winningTile.TileType.Index)) != 0)
        {
          return true;
        }
      }

      // chanta
      // junchan always fulfills chanta
      if ((andFlags & (1L << 5)) != 0 && chantaMelds)
      {
        return true;
      }

      // sanshoku doujun
      // sanshoku doukou
      if ((andFlags & (0b111111111_1111111L << 15)) != 0)
      {
        return true;
      }

      // sanankou
      var ankouCount = seat.Melds.Count(m => m.MeldType == MeldType.ClosedKan) + tileTypeCounts.Skip(27).Count(c => c == 3);
      for (var i = 0; i < 3; i++)
      {
        if (winningTile.TileType.SuitId == i)
        {
          ankouCount += (int) ((flagsBySuit[i] >> (34 + 3 * winningTile.TileType.Index)) & 0b111);
        }
        else
        {
          ankouCount += (int) ((flagsBySuit[i] >> 31) & 0b111);
        }
      }

      if (ankouCount >= 3)
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