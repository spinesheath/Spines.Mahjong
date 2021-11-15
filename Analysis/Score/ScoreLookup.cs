using System;
using System.Linq;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Score
{
  internal static class ScoreLookup
  {
    public static (long, int) Flags(HandCalculator hand, TileType winningTile, bool isRon, int roundWind, int seatWind)
    {
      var data = hand.ScoringData;
      var openBit = data.OpenBit;

      // TODO might be able to rework ron shift to not use up so many bits.
      var ronShiftAmount = isRon ? 9 : 0;

      var winningTileIndex = winningTile.Index;
      var winningTileSuit = winningTile.SuitId;

      var waitShiftValues = data.WaitShiftValues.ToArray();
      waitShiftValues[winningTileSuit] >>= winningTileIndex + 1;

      var suitOr = data.SuitOr;
      var suitsAnd = suitOr[0] & suitOr[1] & suitOr[2];
      var honorOr = data.HonorOr;
      var honorSum = data.HonorSum;

      var bigSum = (suitOr[0] & SuitBigSumFilter) +
                   (suitOr[1] & SuitBigSumFilter) +
                   (suitOr[2] & SuitBigSumFilter) +
                   (honorOr & HonorBigSumFilter);

      var sanshokuShift = (int) suitsAnd & 0b11111;
      var sanshoku = (suitsAnd >> sanshokuShift) & SanshokuYakuFilter;

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
      var windShiftAmount = WindShiftHonor(roundWind, seatWind);
      var honorWindShift = honorOr >> windShiftAmount;
      var waitAndWindShift = waitShiftValues[0] & waitShiftValues[1] & waitShiftValues[2] & waitShiftValues[3] & honorWindShift;
      var pinfu = waitAndWindShift &
                  // TODO suitsAnd + 1 should instead be handled in bitField preparation
                  (suitOr[winningTileSuit] >> (int) ((winningTileIndex + ((suitsAnd + 1) & 1)) * (sanshoku >> 6))) &
                  bigSum &
                  PinfuYakuFilter;

      var tankiBit = waitShiftValues[winningTileSuit] & 0b1L;

      // get this before ron shifting
      // TODO get rid of this conditional
      var singleWaitFuJihai = winningTileSuit == 3 ? (waitShiftValues[winningTileSuit] >> 9) & 2L : 0;
      
      waitShiftValues[winningTileSuit] >>= ronShiftAmount;

      var waitAndRonShift = (waitShiftValues[0] & RonShiftSumFilter) +
                            (waitShiftValues[1] & RonShiftSumFilter) +
                            (waitShiftValues[2] & RonShiftSumFilter) +
                            (waitShiftValues[3] & RonShiftSumFilter);

      waitAndRonShift += data.ShiftedAnkanCount;

      waitAndRonShift += bigSum & (0b111L << AnkouRonShiftSumFilterIndex);
      waitAndRonShift += waitAndRonShift & (0b101L << AnkouRonShiftSumFilterIndex);

      var bigAnd = suitsAnd & honorOr;

      var result = 0L;

      result |= ((1L << BitIndex.MenzenTsumo) >> ronShiftAmount) & (1L << BitIndex.MenzenTsumo);
      result |= waitAndRonShift & AnkouYakuFilter;
      result |= sanshoku;
      result |= pinfu;
      result |= bigAnd & BigAndYakuFilter;

      bigSum |= bigAnd & data.BigAndToSumFilter;

      var bigSumPostElimination = bigSum & ~((bigSum & BigSumEliminationFilter) >> EliminationDelta);
      result |= bigSumPostElimination & BigSumPostEliminationYakuFilter;

      var valueWindFilter = ValueWindFilter(roundWind, seatWind);
      result |= honorSum & HonorSumYakuFilter & valueWindFilter;

      result |= data.SankantsuSuukantsu & (11L << BitIndex.Sankantsu);

      var ryuuiisouSum = (suitOr[0] & RyuuiisouSumFilter01) +
                         (suitOr[1] & RyuuiisouSumFilter01) +
                         (suitOr[2] & RyuuiisouSumFilter2) +
                         honorSum;
      result |= ryuuiisouSum & (1L << BitIndex.Ryuuiisou);

      if ((result & (1L << BitIndex.Chiitoitsu)) != 0)
      {
        result &= ~((1L << BitIndex.ClosedChanta) | (1L << BitIndex.Iipeikou));
      }

      var openIipeikouBit = (result >> BitIndex.Iipeikou) & 1L;
      var sanankouBit = (result >> BitIndex.Sanankou) & 1L;

      // get this before shifting to open ssk
      var sanshokuFuMultiplier = (int) ((result >> BitIndex.SanshokuDoukou) + (result >> BitIndex.ClosedSanshokuDoujun) & 1);

      result += (result & OpenBitFilter) * openBit;

      var closedChantaBit = (result >> BitIndex.ClosedChanta) & 1L;
      var closedJunchanBit = (result >> BitIndex.ClosedJunchan) & 1L;
      var openJunchanBit = (result >> BitIndex.OpenJunchan) & 1L;
      var toitoiBit = (result >> BitIndex.Toitoi) & 1L;

      var x = openIipeikouBit & (closedChantaBit | closedJunchanBit);
      var removeSequenceYaku = (sanankouBit ^ x) & sanankouBit;
      var removeOpenJunchan = openIipeikouBit & (sanankouBit | toitoiBit) & openJunchanBit;
      var removeSanankou = x * (1 - toitoiBit);
      result -= (result & (1L << BitIndex.Sanankou)) * removeSanankou;
      // (openIipeikouBit << BitIndex.OpenChanta) means 111222333 shape and chanta, here excluded in case of sanankou
      result -= (result & ((1L << BitIndex.Pinfu) | (1L << BitIndex.Iipeikou) | (openIipeikouBit << BitIndex.OpenChanta))) * removeSequenceYaku;
      result -= (result & (1L << BitIndex.OpenJunchan)) * removeOpenJunchan;
      result -= (result & ((1L << BitIndex.Iipeikou) | (1L << BitIndex.ClosedJunchan))) * (toitoiBit & (1 - openBit));

      result += (result & TankiUpgradeableFilter) * tankiBit;

      // This covers the 22234555m222p222s case where sanankou/sanshoku doukou depend on the wait.
      var sanankouAndDoukou = (suitOr[winningTileSuit] >> (BitIndex.Sanankou - BitIndex.SanshokuDoukou + 1)) & (1L << BitIndex.SanshokuDoukou);
      var waitPreventsDoukou = (suitsAnd >> (winningTileIndex + ronShiftAmount - 9)) & sanankouAndDoukou;
      result -= waitPreventsDoukou & (result >> (BitIndex.Sanankou - BitIndex.SanshokuDoukou));

      result &= data.FinalMask;
      
      var yakuman = result & YakumanFilter;
      if (yakuman != 0)
      {
        return (yakuman, 0);
      }

      if ((result & (1L << BitIndex.Chiitoitsu)) != 0)
      {
        return (result, 25);
      }

      var closedRonFu = (int)(1 - openBit) * (10 >> (9 - ronShiftAmount));

      if ((result & PinfuYakuFilter) != 0)
      {
        return (result, 20 + closedRonFu);
      }
      
      var squareTypeToShuntsu = ((ryuuiisouSum & ~result) >> BitIndex.Sanankou) & ((1L - openBit) | ((result >> BitIndex.OpenJunchan) & 1L));

      var footprintKey = (sanshokuShift + 1) * 40 * sanshokuFuMultiplier;
      footprintKey += (int)squareTypeToShuntsu * 40;
      footprintKey |= ronShiftAmount & 1; // ronShiftAmount is either 0 or 0b1001
      footprintKey |= (int)openBit << 1;

      var keys = new[] {footprintKey, footprintKey, footprintKey, 0};
      keys[winningTileSuit] += (winningTileIndex + 1) * 4;

      var fuM = data.FuFootprint(0, keys[0]);
      var fuP = data.FuFootprint(1, keys[1]);
      var fuS = data.FuFootprint(2, keys[2]);
      
      // TODO jihai single wait and ankou fu the same way as suits, maybe with value wind info instead of ssk, incorporating valuePairFu
      var bonusAnkouCountWinningSuit = (waitShiftValues[winningTileSuit] >> 32) & 1L;
      var potentialBonusAnkouCountWinningSuit = (data.WaitShiftValues[winningTileSuit] >> 32) & 1L;
      var ankouFuCorrection = (potentialBonusAnkouCountWinningSuit - bonusAnkouCountWinningSuit) * ((0b100000 >> winningTileSuit) & 0b100);
      var ankouFuJihai = ((data.WaitShiftValues[3] >> 17) & (0b111L << 3)) - ankouFuCorrection;

      var tsumoFu = 2 >> ronShiftAmount;

      // TODO this can be calculated once together with windShiftAmount and used over many hands
      // doubleValueWindBit is 1 iff seat and round wind are the same
      var doubleValueWindBit = System.Runtime.Intrinsics.X86.Popcnt.PopCount((uint)windShiftAmount) & 1L;
      // lowest bit of honorOr is 1 iff there is a wind pair
      var valueWindAdjuster = 1 + (honorOr & doubleValueWindBit);
      var valuePairFu = (honorWindShift & 1L) << (int)valueWindAdjuster;

      var fu = data.Fu + closedRonFu + tsumoFu + (int) valuePairFu + fuM + fuP + fuS + (int)ankouFuJihai + (int)singleWaitFuJihai;

      var r = fu % 10;
      var rounding = (r == 0 && fu != 20) ? 0 : 10 - r;

      return (result, fu + rounding);
    }

    private const int EliminationDelta = 4;

    private const long SanshokuYakuFilter = (1L << BitIndex.ClosedSanshokuDoujun) | (1L << BitIndex.SanshokuDoukou);

    private const long BigAndYakuFilter = (1L << BitIndex.Honroutou) |
                                          (1L << BitIndex.ClosedTanyao) |
                                          (1L << BitIndex.Chinroutou) |
                                          (1L << BitIndex.ClosedJunchan);

    private const long BigSumPostEliminationYakuFilter = (1L << BitIndex.Iipeikou) | (1L << BitIndex.Chiitoitsu) | (1L << BitIndex.Ryanpeikou) |
                                                         (1L << BitIndex.ClosedChinitsu) | (1L << BitIndex.ClosedHonitsu) |
                                                         (1L << BitIndex.ChuurenPoutou) |
                                                         (1L << BitIndex.ClosedChanta) | (1L << BitIndex.Toitoi) |
                                                         (1L << BitIndex.ClosedIttsuu);

    private const long BigSumEliminationFilter = (1L << (BitIndex.ClosedChinitsu + 4)) | (1L << (BitIndex.OpenChinitsu + 4)) |
                                                 (1L << (BitIndex.ClosedHonitsu + 4)) | (1L << (BitIndex.OpenHonitsu + 4)) |
                                                 (1L << BitIndex.Toitoi);

    private const long HonorSumYakuFilter = (1L << BitIndex.Haku) | (1L << BitIndex.Hatsu) | (1L << BitIndex.Chun) |
                                            (1L << BitIndex.JikazeTon) | (1L << BitIndex.JikazeNan) |
                                            (1L << BitIndex.JikazeShaa) | (1L << BitIndex.JikazePei) |
                                            (1L << BitIndex.BakazeTon) | (1L << BitIndex.BakazeNan) |
                                            (1L << BitIndex.BakazeShaa) | (1L << BitIndex.BakazePei) |
                                            (1L << BitIndex.Shousangen) | (1L << BitIndex.Daisangen) |
                                            (1L << BitIndex.Shousuushi) | (1L << BitIndex.Daisuushi) |
                                            (1L << BitIndex.KokushiMusou) | (1L << BitIndex.Tsuuiisou);

    private const long YakumanFilter = (1L << BitIndex.Daisangen) | (1L << BitIndex.Shousuushi) | (1L << BitIndex.Daisuushi) |
                                       (1L << BitIndex.Suuankou) | (1L << BitIndex.SuuankouTanki) |
                                       (1L << BitIndex.KokushiMusou) | (1L << BitIndex.KokushiMusouJuusanmen) |
                                       (1L << BitIndex.Tsuuiisou) | (1L << BitIndex.Chinroutou) |
                                       (1L << BitIndex.ChuurenPoutou) | (1L << BitIndex.JunseiChuurenPoutou) |
                                       (1L << BitIndex.Suukantsu) | (1L << BitIndex.Ryuuiisou);

    private const long PinfuYakuFilter = 1L << BitIndex.Pinfu;

    private const int AnkouRonShiftSumFilterIndex = BitIndex.Sanankou - 2;
    private const long RonShiftSumFilter = 1L << AnkouRonShiftSumFilterIndex;
    private const long AnkouYakuFilter = (1L << BitIndex.Sanankou) | (1L << BitIndex.Suuankou) | (1L << BitIndex.SuuankouTanki);

    private const long SuitBigSumFilter = (0b11_00000_0101_0000L << 19) |
                                          (1L << (BitIndex.Pinfu - 1)) |
                                          (1L << BitIndex.ChuurenPoutou) |
                                          (0b1111L << (BitIndex.Chiitoitsu - 3)) |
                                          (0b11L << BitIndex.Iipeikou) |
                                          (1L << BitIndex.ClosedIttsuu);

    private const long HonorBigSumFilter = (0b11_00000_0000_1111L << 19) |
                                           (1L << BitIndex.Pinfu) |
                                           (0b1111L << (BitIndex.Chiitoitsu - 3));

    private const long TankiUpgradeableFilter = (1L << BitIndex.Suuankou) | (1L << BitIndex.KokushiMusou) | (1L << BitIndex.ChuurenPoutou);

    private const long OpenBitFilter = (1L << BitIndex.ClosedChinitsu) | (1L << BitIndex.ClosedHonitsu) | (1L << BitIndex.ClosedSanshokuDoujun) |
                                       (1L << BitIndex.ClosedTanyao) | (1L << BitIndex.ClosedChanta) | (1L << BitIndex.ClosedJunchan) |
                                       (1L << BitIndex.ClosedIttsuu);

    // Sanankou bit index added here for square type, since it does not interfere with ryuuiisou
    private const long RyuuiisouSumFilter01 = (1L << (BitIndex.Ryuuiisou - 4)) | (1L << BitIndex.Sanankou);
    private const long RyuuiisouSumFilter2 = (1L << (BitIndex.Ryuuiisou - 2)) | (1L << BitIndex.Sanankou);

    private static int WindShiftHonor(int roundWind, int seatWind)
    {
      return (1 << roundWind) | (1 << seatWind);
    }

    private static long ValueWindFilter(int roundWind, int seatWind)
    {
      var mask = ~((0b1111L << BitIndex.BakazeTon) | (0b1111L << BitIndex.JikazeTon));
      mask |= 0b1L << (BitIndex.BakazeTon + roundWind);
      mask |= 0b1L << (BitIndex.JikazeTon + seatWind);
      return mask;
    }

    private static string PrintBinarySegment(long bits, int from, int length)
    {
      return Convert.ToString((bits >> from) & ((1L << length) - 1), 2).PadLeft(length, '0');
    }
  }
}