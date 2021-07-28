using System;
using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Resources;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Score
{
  internal static class ScoreLookup
  {
    static ScoreLookup()
    {
      HonorSumLookup = Resource.LongLookup("Scoring", "HonorSumLookup.dat");
      HonorMeldSumLookup = Resource.LongLookup("Scoring", "HonorMeldSumLookup.dat");
      HonorWaitShiftLookup = Resource.LongLookup("Scoring", "HonorWaitShiftLookup.dat");

      SuitOrLookup = Resource.LongLookup("Scoring", "SuitOrLookup.dat");
      SuitMeldOrLookup = Resource.LongLookup("Scoring", "SuitMeldOrLookup.dat");
      SuitWaitShiftLookup = Resource.LongLookup("Scoring", "SuitWaitShiftLookup.dat");
    }

    private static long HonorSum(int concealedIndex, int meldIndex)
    {
      var concealed = HonorSumLookup[concealedIndex];
      var melded = HonorMeldSumLookup[meldIndex];
      return concealed + melded;
    }

    private static long SuitOr(HandCalculator hand, int suitId)
    {
      var concealedIndex = hand.Base5Hash(suitId);
      var meldIndex = MeldIndex(hand, suitId);
      return SuitOr(concealedIndex, meldIndex);
    }

    private static long SuitOr(int concealedIndex, int meldIndex)
    {
      var concealed = SuitOrLookup[concealedIndex];
      var melded = SuitMeldOrLookup[meldIndex];
      return concealed | melded;
    }

    private static long SuitWaitShift(HandCalculator hand, int suitId)
    {
      return SuitWaitShiftLookup[hand.Base5Hash(suitId)];
    }

    private static long HonorWaitShift(HandCalculator hand)
    {
      return HonorWaitShiftLookup[hand.Base5Hash(3)];
    }

    private static readonly long[] HonorSumLookup;
    private static readonly long[] HonorMeldSumLookup;
    private static readonly long[] HonorWaitShiftLookup;
    private static readonly long[] SuitOrLookup;
    private static readonly long[] SuitMeldOrLookup;
    private static readonly long[] SuitWaitShiftLookup;

    private const int IipeikouDelta = 4;
    private const int IipeikouBitIndex = 37;
    private const int ChiitoitsuBitIndex = IipeikouBitIndex + IipeikouDelta;
    private const int RyanpeikouBitIndex = ChiitoitsuBitIndex + IipeikouDelta;

    private static int MeldIndex(HandCalculator hand, int suitId)
    {
      var meldIndex = 0;
      foreach (var i in hand.MeldIds(suitId))
      {
        meldIndex *= 35;
        meldIndex += i + 1;
      }

      return meldIndex;
    }

    private static int WindShiftHonor(int roundWind, int seatWind)
    {
      return 1 << roundWind | 1 << seatWind;
    }

    private static long ValueWindFilter(int roundWind, int seatWind)
    {
      var mask = ~(0b1111L << BitIndex.BakazeTon | 0b1111L << BitIndex.JikazeTon);
      mask |= 0b1L << (BitIndex.BakazeTon + roundWind);
      mask |= 0b1L << (BitIndex.JikazeTon + seatWind);
      return mask;
    }

    // TODO bitshift of long is arithmetic: sign bit always stays - either make use of that or use ulong
    public static long Flags2(HandCalculator hand, Tile winningTile, bool isRon, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds)
    {
      var isOpen = melds.Any(m => !m.IsKan || m.CalledTile != null);
      var hasChii = melds.Any(m => m.MeldType == MeldType.Shuntsu);
      var hasChantaCalls = melds.Any(m => m.Tiles.Any(t => t.TileType.IsKyuuhai));
      
      var honorConcealedIndex = hand.Base5Hash(3);
      var honorMeldIndex = MeldIndex(hand, 3);
      
      var shiftedAnkanCount = (long)melds.Count(m => m.IsKan && m.CalledTile == null) << (BitIndex.Sanankou - 2);
      var waitShiftValues = new [] {SuitWaitShift(hand, 0), SuitWaitShift(hand, 1), SuitWaitShift(hand, 2), HonorWaitShift(hand)};
      var concealedOrMeldedValues = new[] { SuitOr(hand, 0), SuitOr(hand, 1), SuitOr(hand, 2) };
      var honorSum = HonorSum(honorConcealedIndex, honorMeldIndex);

      var valueWindFilter = ValueWindFilter(roundWind, seatWind);
      var honorWindShiftAmount = WindShiftHonor(roundWind, seatWind);

      var honorWaitAndWindShift = waitShiftValues[3] >> honorWindShiftAmount;

      waitShiftValues[winningTile.TileType.SuitId] >>= winningTile.TileType.Index + 1;

      // TODO open closed similar to tanki bit? -> x += x * open bit -> for sanshoku, honitsu, chinitsu (and some more)?

      // TODO could rightshift noHonorTanki by the total pair count?
      var noHonorTanki = (0b111L << BitIndex.Pinfu) >> winningTile.TileType.SuitId;
      var waitAndWindShift = waitShiftValues[0] & waitShiftValues[1] & waitShiftValues[2] & honorWaitAndWindShift & noHonorTanki;
      var tankiBit = waitShiftValues[winningTile.TileType.SuitId] & 0b1L;

      waitShiftValues[winningTile.TileType.SuitId] >>= isRon ? 9 : 0;

      var bigSum = (concealedOrMeldedValues[0] & SuitBigSumFilter) + 
                   (concealedOrMeldedValues[1] & SuitBigSumFilter) + 
                   (concealedOrMeldedValues[2] & SuitBigSumFilter) + 
                   (honorSum & HonorBigSumFilter);

      var waitAndRonShift = (waitShiftValues[0] & RonShiftSumFilter) + 
                            (waitShiftValues[1] & RonShiftSumFilter) + 
                            (waitShiftValues[2] & RonShiftSumFilter) +
                            (waitShiftValues[3] & RonShiftSumFilter);
      waitAndRonShift += shiftedAnkanCount;
      
      waitAndRonShift += bigSum & (0b111L << AnkouRonShiftSumFilterIndex);
      waitAndRonShift += waitAndRonShift & (0b101L << AnkouRonShiftSumFilterIndex);
      
      var suuankouBit = (waitAndRonShift | honorSum) & TankiUpgradeableFilter;

      var suitsAnd = concealedOrMeldedValues[0] & concealedOrMeldedValues[1] & concealedOrMeldedValues[2];

      var result = 0L;

      result |= honorSum & HonorSumYakuFilter & valueWindFilter;

      result |= waitAndWindShift & WaitShiftYakuFilter;
      
      result |= waitAndRonShift & WaitAndRonShiftYakuFilter;
      result += tankiBit * suuankouBit;
      
      result |= (suitsAnd >> (int)suitsAnd) & SanshokuYakuFilter;

      result |= suitsAnd & honorSum & AllAndYakuFilter;

      var iipeikouPostElimination = bigSum & ~(((bigSum & IipeikouEliminationFilter) | ((waitAndRonShift & 0b1L << BitIndex.Sanankou) << (3 + IipeikouDelta))) >> IipeikouDelta);

      result |= iipeikouPostElimination & IipeikouYakuFilter;

      var yakuman = result & YakumanFilter;
      if (yakuman != 0)
      {
        return yakuman;
      }

      if (isOpen)
      {
        result &= ClosedYakuFilter;
      }
      else
      {
        result &= OpenYakuFilter;
      }

      if (hasChii)
      {
        result &= NoChiiYakuFilter;
      }

      if (hasChantaCalls)
      {
        result &= NoChantaCallsFilter;
      }

      if (shiftedAnkanCount != 0)
      {
        result &= NoAnkanYakuFilter;
      }

      return result;
    }

    private static string PrintBinarySegment(long bits, int from, int length)
    {
      return Convert.ToString((bits >> from) & ((1L << length) - 1), 2).PadLeft(length, '0');
    }

    private const long SanshokuYakuFilter = (0b1L << BitIndex.ClosedSanshokuDoujun) | (0b1L << BitIndex.OpenSanshokuDoujun) | (0b1L << BitIndex.SanshokuDoukou);
    private const long AllAndYakuFilter = (0b1L << BitIndex.Toitoi) | (0b1L << BitIndex.ClosedTanyao) | (0b1L << BitIndex.OpenTanyao);
    private const long IipeikouYakuFilter = (0b1L << IipeikouBitIndex) | (0b1L << ChiitoitsuBitIndex) | (0b1L << RyanpeikouBitIndex) |
                                            0b1L << BitIndex.ClosedChinitsu| 0b1L << BitIndex.OpenChinitsu |
                                            0b1L << BitIndex.ClosedHonitsu | 0b1L << BitIndex.OpenHonitsu;
    private const long IipeikouEliminationFilter = (0b1L << ChiitoitsuBitIndex) | (0b1L << RyanpeikouBitIndex) |
                                                   0b1L << (BitIndex.ClosedChinitsu + 4) | 0b1L << (BitIndex.OpenChinitsu + 4) |
                                                   0b1L << (BitIndex.ClosedHonitsu + 4) | 0b1L << (BitIndex.OpenHonitsu + 4);

    private const long HonorSumYakuFilter = (0b1L << BitIndex.Haku) | (0b1L << BitIndex.Hatsu) | (0b1L << BitIndex.Chun) |
                                           (0b1L << BitIndex.JikazeTon) | (0b1L << BitIndex.JikazeNan) | (0b1L << BitIndex.JikazeShaa) | (0b1L << BitIndex.JikazePei) |
                                           (0b1L << BitIndex.BakazeTon) | (0b1L << BitIndex.BakazeNan) | (0b1L << BitIndex.BakazeShaa) | (0b1L << BitIndex.BakazePei) |
                                           (0b1L << BitIndex.Shousangen) | (0b1L << BitIndex.Daisangen) | (0b1L << BitIndex.Shousuushi) | (0b1L << BitIndex.Daisuushi) |
                                           (0b1L << BitIndex.KokushiMusou);

    private const long YakumanFilter = (0b1L << BitIndex.Daisangen) | (0b1L << BitIndex.Shousuushi) | (0b1L << BitIndex.Daisuushi) | 
                                       (0b1L << BitIndex.Suuankou) | (0b1L << BitIndex.SuuankouTanki) |
                                       (0b1L << BitIndex.KokushiMusou) | (0b1L << BitIndex.KokushiMusouJuusanmen);
    private const long ClosedYakuFilter = ~((0b1L << BitIndex.ClosedSanshokuDoujun) | (0b1L << BitIndex.Iipeikou) | 
                                            (0b1L << BitIndex.Chiitoitsu) | (0b1L << BitIndex.Ryanpeikou) |
                                            (0b1L << BitIndex.ClosedHonitsu) | (0b1L << BitIndex.ClosedChinitsu) | 
                                            (0b1L << BitIndex.ClosedTanyao) | (0b1L << BitIndex.MenzenTsumo) |
                                            (0b1L << BitIndex.Pinfu));
    private const long OpenYakuFilter = ~((0b1L << BitIndex.OpenSanshokuDoujun) | (0b1L << BitIndex.OpenHonitsu) | (0b1L << BitIndex.OpenChinitsu) |
                                          (0b1L << BitIndex.OpenTanyao));
    private const long NoChiiYakuFilter = ~((0b1L << BitIndex.Toitoi));
    private const long NoAnkanYakuFilter = ~(0b1L << BitIndex.Pinfu);

    private const long WaitAndRonShiftYakuFilter = (0b1L << BitIndex.Sanankou) | (0b1L << BitIndex.Suuankou) | (0b1L << BitIndex.SuuankouTanki) | (0b1L << BitIndex.MenzenTsumo);
    private const long WaitShiftYakuFilter = (0b1L << BitIndex.Pinfu);
    private const long RonShiftSumFilter = (0b1L << AnkouRonShiftSumFilterIndex) | (0b1L << BitIndex.MenzenTsumo - 2);
    private const int AnkouRonShiftSumFilterIndex = BitIndex.Sanankou - 2;

    private const long NoChantaCallsFilter = ~((0b1L << BitIndex.ClosedTanyao) | (0b1L << BitIndex.OpenTanyao));

    private const long SuitBigSumFilter = 0b11111111_0_11111111_11111111111111L << 32;
    private const long HonorBigSumFilter = 0b11111111_0_11111111_00001111111111L << 32;
    
    private const long TankiUpgradeableFilter = (0b1L << BitIndex.Suuankou) | (0b1L << BitIndex.KokushiMusou);
  }
}