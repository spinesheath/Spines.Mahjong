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
      HonorOrLookup = Resource.LongLookup("Scoring", "HonorOrLookup.dat");
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

    private static long HonorOr(int concealedIndex, int meldIndex)
    {
      var concealed = HonorOrLookup[concealedIndex];
      var melded = HonorMeldSumLookup[meldIndex];
      return concealed | melded;
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
      var suitOr = concealed | melded;
      suitOr += 1L << (BitIndex.ClosedIttsuu - 3);
      return suitOr;
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
    private static readonly long[] HonorOrLookup;
    private static readonly long[] HonorMeldSumLookup;
    private static readonly long[] HonorWaitShiftLookup;
    private static readonly long[] SuitOrLookup;
    private static readonly long[] SuitMeldOrLookup;
    private static readonly long[] SuitWaitShiftLookup;

    private const int EliminationDelta = 4;

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
    
    // TODO is the distinction between Open/Closed Kan / Pon necessary for meld lookups? Ankan count is applied otherwise. But meld lookup should be replaced anyways

    public static long Flags(HandCalculator hand, Tile winningTile, bool isRon, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds)
    {
      var isOpen = melds.Any(m => !m.IsKan || m.CalledTile != null);
      var hasChii = melds.Any(m => m.MeldType == MeldType.Shuntsu);
      var hasChantaCalls = melds.Any(m => m.Tiles.Any(t => t.TileType.IsKyuuhai));
      var hasNonChantaCalls = melds.Any(m => !m.Tiles.Any(t => t.TileType.IsKyuuhai));
      var hasNonChinroutouCalls = melds.Any(m => m.Tiles.Any(t => t.TileType.Suit == Suit.Jihai || t.TileType.Index > 0 && t.TileType.Index < 8));
      var hasNonHonroutouCalls = melds.Any(m => m.Tiles.Any(t => t.TileType.Suit != Suit.Jihai && t.TileType.Index > 0 && t.TileType.Index < 8));
      var hasHonorCalls = melds.Any(m => m.Tiles.Any(t => t.TileType.Suit == Suit.Jihai));
      var kanCount = melds.Count(m => m.IsKan);

      var honorConcealedIndex = hand.Base5Hash(3);
      var honorMeldIndex = MeldIndex(hand, 3);
      
      var shiftedAnkanCount = (long)melds.Count(m => m.IsKan && m.CalledTile == null) << (BitIndex.Sanankou - 2);
      var ankanYakuFilter = shiftedAnkanCount != 0 ? NoAnkanYakuFilter : ~0L;

      var waitShiftValues = new [] {SuitWaitShift(hand, 0), SuitWaitShift(hand, 1), SuitWaitShift(hand, 2), HonorWaitShift(hand)};
      waitShiftValues[winningTile.TileType.SuitId] >>= winningTile.TileType.Index + 1;

      var suitOr = new[] { SuitOr(hand, 0), SuitOr(hand, 1), SuitOr(hand, 2), ~0L };
      var suitsAnd = suitOr[0] & suitOr[1] & suitOr[2];
      var honorOr = HonorOr(honorConcealedIndex, honorMeldIndex);

      var bigSum = (suitOr[0] & SuitBigSumFilter) +
                   (suitOr[1] & SuitBigSumFilter) +
                   (suitOr[2] & SuitBigSumFilter) +
                   (honorOr & HonorBigSumFilter) & ankanYakuFilter;

      var sanshoku = (suitsAnd >> (int)suitsAnd) & SanshokuYakuFilter;

      /*
       * Pinfu
       * Honors are shifted by an amount based on value winds to make sure only guest wind pairs are possible
       * The suit with the winning tile is shifted by the drawn tile to ensure ryanmen wait and non-honor wait (also used for other yaku)
       * After that, some constellations where pinfu is not possible because of other yaku locking shapes are eliminated:
       * Sanankou and sanshoku. Ittsuu is a single suit issue and has been dealt with in the lookup preparation.
       * Some chiitoitsu hands evaluate to pinfu by the previous steps, despite being clearly not pinfu.
       * A flag in BigSum is created by adding 1 for each suit with a pair and a 2 for honors.
       * This will leave a 0 in the second bit in the bad case: 11223399m11p11s44z This 0 is aligned with the pinfu bit index.
       */
      var honorWindShift = honorOr >> WindShiftHonor(roundWind, seatWind);
      var waitAndWindShift = waitShiftValues[0] & waitShiftValues[1] & waitShiftValues[2] & waitShiftValues[3] & honorWindShift;
      var pinfu = waitAndWindShift & 
                  (suitOr[winningTile.TileType.SuitId] >> (int)((winningTile.TileType.Index + (suitsAnd & 1)) * (sanshoku >> 2))) & 
                  bigSum &
                  PinfuYakuFilter & 
                  ankanYakuFilter;

      var tankiBit = waitShiftValues[winningTile.TileType.SuitId] & 0b1L;
      var openBit = isOpen ? 1L : 0L;

      waitShiftValues[winningTile.TileType.SuitId] >>= isRon ? 9 : 0;

      var waitAndRonShift = (waitShiftValues[0] & RonShiftSumFilter) + 
                            (waitShiftValues[1] & RonShiftSumFilter) + 
                            (waitShiftValues[2] & RonShiftSumFilter) +
                            (waitShiftValues[3] & RonShiftSumFilter);
      waitAndRonShift += shiftedAnkanCount;
      
      waitAndRonShift += bigSum & (0b111L << AnkouRonShiftSumFilterIndex);
      waitAndRonShift += waitAndRonShift & (0b101L << AnkouRonShiftSumFilterIndex);
      
      var result = 0L;
      
      result |= waitAndRonShift & WaitAndRonShiftYakuFilter;

      result |= sanshoku;
      var bigAnd = suitsAnd & honorOr;

      result |= bigAnd & BigAndYakuFilter;

      result |= pinfu;

      if (hasChii)
      {
        bigAnd &= NoChiiYakuFilter;
      }
      
      bigSum |= bigAnd & ((0b1L << BitIndex.Toitoi) | (0b1L << BitIndex.ClosedChanta));

      // TODO this currently only affects toitoi => chanta and honitsu?
      var bigSumPostElimination = bigSum & ~((bigSum & BigSumEliminationFilter) >> EliminationDelta);
      result |= bigSumPostElimination & BigSumPostEliminationYakuFilter;

      if ((result & (1L << BitIndex.Chiitoitsu)) != 0)
      {
        result &= ~((1L << BitIndex.ClosedChanta | 1L << BitIndex.Iipeikou));
      }

      var iipeikouBit = (result >> BitIndex.Iipeikou) & 1L;
      var sanankouBit = (result >> BitIndex.Sanankou) & 1L;
      
      result += (result & OpenBitFilter) * openBit;

      var closedChantaBit = (result >> BitIndex.ClosedChanta) & 1L;
      var closedJunchanBit = (result >> BitIndex.ClosedJunchan) & 1L;
      var openJunchanBit = (result >> BitIndex.OpenJunchan) & 1L;

      var x = iipeikouBit & (closedChantaBit | closedJunchanBit);
      var y = (sanankouBit ^ x) & sanankouBit;
      var z = iipeikouBit & sanankouBit & openJunchanBit;
      result -= (result & (1L << BitIndex.Sanankou)) * x;
      result -= (result & ((1L << BitIndex.Pinfu) | (1L << BitIndex.Iipeikou))) * y;
      result -= (result & (1L << BitIndex.OpenJunchan)) * z;

      var honorSum = HonorSum(honorConcealedIndex, honorMeldIndex);
      var valueWindFilter = ValueWindFilter(roundWind, seatWind);
      result |= honorSum & HonorSumYakuFilter & valueWindFilter;

      result += (result & TankiUpgradeableFilter) * tankiBit;
      result |= (1L << (kanCount + BitIndex.Sankantsu - 3)) & (11L << BitIndex.Sankantsu);

      if (hasNonChinroutouCalls)
      {
        result &= ChinroutouCallFilter;
      }

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

      if (hasChantaCalls)
      {
        result &= NoChantaCallsFilter;
      }

      if (hasNonChantaCalls)
      {
        result &= OnlyChantaCallsFilter;
      }

      if (hasNonHonroutouCalls)
      {
        result &= HonroutouCallFilter;
      }

      if (hasHonorCalls)
      {
        result &= HonorCallFilter;
      }

      return result;
    }

    private static string PrintBinarySegment(long bits, int from, int length)
    {
      return Convert.ToString((bits >> from) & ((1L << length) - 1), 2).PadLeft(length, '0');
    }

    private const long SanshokuYakuFilter = (0b1L << BitIndex.ClosedSanshokuDoujun) | (0b1L << BitIndex.SanshokuDoukou);
    private const long BigAndYakuFilter = (0b1L << BitIndex.Honroutou) | 
                                          (0b1L << BitIndex.ClosedTanyao) |
                                          (0b1L << BitIndex.Chinroutou) |
                                          (0b1L << BitIndex.ClosedJunchan);

    private const long BigSumPostEliminationYakuFilter = (0b1L << BitIndex.Iipeikou) | (0b1L << BitIndex.Chiitoitsu) | (0b1L << BitIndex.Ryanpeikou) |
                                                         (0b1L << BitIndex.ClosedChinitsu) | (0b1L << BitIndex.ClosedHonitsu) |
                                                         (0b1L << BitIndex.ChuurenPoutou) |
                                                         (0b1L << BitIndex.ClosedChanta) | (0b1L << BitIndex.Toitoi) |
                                                         (0b1L << BitIndex.ClosedIttsuu);

    private const long BigSumEliminationFilter = (0b1L << (BitIndex.ClosedChinitsu + 4)) | (0b1L << (BitIndex.OpenChinitsu + 4)) |
                                                 (0b1L << (BitIndex.ClosedHonitsu + 4)) | (0b1L << (BitIndex.OpenHonitsu + 4)) |
                                                 (0b1L << BitIndex.Toitoi);

    private const long HonorSumYakuFilter = (0b1L << BitIndex.Haku) | (0b1L << BitIndex.Hatsu) | (0b1L << BitIndex.Chun) |
                                            (0b1L << BitIndex.JikazeTon) | (0b1L << BitIndex.JikazeNan) | 
                                            (0b1L << BitIndex.JikazeShaa) | (0b1L << BitIndex.JikazePei) |
                                            (0b1L << BitIndex.BakazeTon) | (0b1L << BitIndex.BakazeNan) |
                                            (0b1L << BitIndex.BakazeShaa) | (0b1L << BitIndex.BakazePei) |
                                            (0b1L << BitIndex.Shousangen) | (0b1L << BitIndex.Daisangen) |
                                            (0b1L << BitIndex.Shousuushi) | (0b1L << BitIndex.Daisuushi) |
                                            (0b1L << BitIndex.KokushiMusou) | (0b1L << BitIndex.Tsuuiisou);

    private const long YakumanFilter = (0b1L << BitIndex.Daisangen) | (0b1L << BitIndex.Shousuushi) | (0b1L << BitIndex.Daisuushi) | 
                                       (0b1L << BitIndex.Suuankou) | (0b1L << BitIndex.SuuankouTanki) |
                                       (0b1L << BitIndex.KokushiMusou) | (0b1L << BitIndex.KokushiMusouJuusanmen) |
                                       (0b1L << BitIndex.Tsuuiisou) | (0b1L << BitIndex.Chinroutou) |
                                       (0b1L << BitIndex.ChuurenPoutou) | (0b1L << BitIndex.JunseiChuurenPoutou) |
                                       (0b1L << BitIndex.Suukantsu);
    private const long ClosedYakuFilter = ~((0b1L << BitIndex.ClosedSanshokuDoujun) | (0b1L << BitIndex.Iipeikou) |
                                            (0b1L << BitIndex.Chiitoitsu) | (0b1L << BitIndex.Ryanpeikou) |
                                            (0b1L << BitIndex.ClosedHonitsu) | (0b1L << BitIndex.ClosedChinitsu) |
                                            (0b1L << BitIndex.ClosedTanyao) | (0b1L << BitIndex.MenzenTsumo) |
                                            (0b1L << BitIndex.Pinfu) | (0b1L << BitIndex.ClosedChanta) |
                                            (0b1L << BitIndex.ClosedJunchan) | (0b1L << BitIndex.ClosedIttsuu));
    private const long OpenYakuFilter = ~((0b1L << BitIndex.OpenSanshokuDoujun) | (0b1L << BitIndex.OpenHonitsu) | (0b1L << BitIndex.OpenChinitsu) |
                                          (0b1L << BitIndex.OpenTanyao) | (0b1L << BitIndex.OpenChanta) | (0b1L << BitIndex.OpenJunchan) |
                                          (0b1L << BitIndex.OpenIttsuu));

    private const long NoChiiYakuFilter = ~((0b1L << BitIndex.Toitoi));
    private const long NoAnkanYakuFilter = ~((0b1L << BitIndex.Pinfu) | (0b1L << BitIndex.Chiitoitsu));

    private const long WaitAndRonShiftYakuFilter = (0b1L << BitIndex.Sanankou) | (0b1L << BitIndex.Suuankou) | (0b1L << BitIndex.SuuankouTanki) | (0b1L << BitIndex.MenzenTsumo);
    private const long PinfuYakuFilter = 0b1L << BitIndex.Pinfu;
    private const long RonShiftSumFilter = (0b1L << AnkouRonShiftSumFilterIndex) | (0b1L << BitIndex.MenzenTsumo - 2);
    private const int AnkouRonShiftSumFilterIndex = BitIndex.Sanankou - 2;

    private const long NoChantaCallsFilter = ~((0b1L << BitIndex.ClosedTanyao) | (0b1L << BitIndex.OpenTanyao));
    private const long OnlyChantaCallsFilter = ~((0b1L << BitIndex.ClosedChanta) | (0b1L << BitIndex.OpenChanta) |
                                                 (0b1L << BitIndex.ClosedJunchan) | (0b1L << BitIndex.OpenJunchan));
    private const long ChinroutouCallFilter = ~(0b1L << BitIndex.Chinroutou);
    private const long HonroutouCallFilter = ~(0b1L << BitIndex.Honroutou);
    private const long HonorCallFilter = ~((0b1L << BitIndex.ClosedChinitsu) | (0b1L << BitIndex.OpenChinitsu) | 
                                           (0b1L << BitIndex.ClosedJunchan) | (0b1L << BitIndex.OpenJunchan));
    
    private const long SuitBigSumFilter = (0b11_00000_0101_0000L << 19) | 
                                          (0b1L << (BitIndex.Pinfu - 1)) | 
                                          (0b1L << BitIndex.ChuurenPoutou) | 
                                          (0b1111L << (BitIndex.Chiitoitsu - 3)) |
                                          (0b11L << BitIndex.Iipeikou) |
                                          (0b1L << BitIndex.ClosedIttsuu);
    private const long HonorBigSumFilter = (0b11_00000_0000_1111L << 19) | 
                                           (0b1L << BitIndex.Pinfu) | 
                                           (0b1111L << (BitIndex.Chiitoitsu - 3));
    
    private const long TankiUpgradeableFilter = (0b1L << BitIndex.Suuankou) | (0b1L << BitIndex.KokushiMusou) | (0b1L << BitIndex.ChuurenPoutou);
    private const long OpenBitFilter = (0b1L << BitIndex.ClosedChinitsu) | (0b1L << BitIndex.ClosedHonitsu) | (0b1L << BitIndex.ClosedSanshokuDoujun) |
                                       (0b1L << BitIndex.ClosedTanyao) | (0b1L << BitIndex.ClosedChanta) | (0b1L << BitIndex.ClosedJunchan) |
                                       (0b1L << BitIndex.ClosedIttsuu);
  }
}