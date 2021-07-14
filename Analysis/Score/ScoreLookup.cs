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

      SuitOrLookup = Resource.LongLookup("Scoring", "SuitOrLookup.dat");
      SuitMeldOrLookup = Resource.LongLookup("Scoring", "SuitMeldOrLookup.dat");
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

    private static readonly long[] HonorSumLookup;
    private static readonly long[] HonorMeldSumLookup;
    private static readonly long[] SuitOrLookup;
    private static readonly long[] SuitMeldOrLookup;

    private const int PinfuBitIndex = 10;
    private const int KokushiMusouJuusanMenBitIndex = 0;
    private const int JunseiChuurenPoutouBitIndex = 22;

    private const int SanankouBitIndex = 34;
    private const int AnkouRonShiftSumFilterIndex = SanankouBitIndex - 2;
    private const int SuuankouBitIndex = SanankouBitIndex + 1;
    private const int SuuankouTankiBitIndex = SuuankouBitIndex + 1;

    private const int MenzenTsumoBitIndex = 56;
    private const int MenzenTsumoRonShiftSumFilterIndex = MenzenTsumoBitIndex - 2;

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

    private static long ValueWindFilter(int roundWind, int seatWind)
    {
      var mask = ~(0b1111L << BitIndex.BakazeTon | 0b1111L << BitIndex.JikazeTon | 0b1111L);
      mask |= 0b1L << (BitIndex.BakazeTon + roundWind);
      mask |= 0b1L << (BitIndex.JikazeTon + seatWind);
      mask |= 0b1L << roundWind;
      mask |= 0b1L << seatWind;
      return mask;
    }

    // TODO bitshift of long is arithmetic: sign bit always stays - either make use of that or use ulong
    public static long Flags2(HandCalculator hand, Tile winningTile, bool isRon, int roundWind, int seatWind, bool isOpen)
    {
      var honorConcealedIndex = hand.Base5Hash(3);
      var honorMeldIndex = MeldIndex(hand, 3);
      
      var shiftedAnkanCount = 0b0L;
      var waitShiftValues = new [] {1L, 1L, 1L, 1L};
      var concealedOrMeldedValues = new[] { SuitOr(hand, 0), SuitOr(hand, 1), SuitOr(hand, 2) };
      var iipeikouValues = new[] { 1L, 1L, 1L, 1L };

      var valueWindFilter = ValueWindFilter(roundWind, seatWind);

      waitShiftValues[winningTile.TileType.SuitId] >>= winningTile.TileType.Index + 1;
      var windShiftHonor = 1L >> (int)(valueWindFilter & 0b1111L);

      var waitAndWindShift = waitShiftValues[0] & waitShiftValues[1] & waitShiftValues[2] & waitShiftValues[3] & windShiftHonor;
      var tankiBit = waitShiftValues[winningTile.TileType.SuitId] & 0b1L;

      waitShiftValues[winningTile.TileType.SuitId] >>= isRon ? 9 : 0;

      var ronShiftSumFilter = 0L;
      ronShiftSumFilter |= 0b1L << AnkouRonShiftSumFilterIndex;
      ronShiftSumFilter |= 0b1L << MenzenTsumoRonShiftSumFilterIndex;
      var waitAndRonShift = (waitShiftValues[0] & ronShiftSumFilter) + (waitShiftValues[1] & ronShiftSumFilter) + (waitShiftValues[2] & ronShiftSumFilter) + (waitShiftValues[3] & ronShiftSumFilter);
      waitAndRonShift += shiftedAnkanCount;
      waitAndRonShift += waitAndRonShift & 0b101L << AnkouRonShiftSumFilterIndex;
      var suuankouBit = waitAndRonShift & (0b1L << SuuankouBitIndex);

      var sanshoku = concealedOrMeldedValues[0] & concealedOrMeldedValues[1] & concealedOrMeldedValues[2];
      sanshoku >>= (int)sanshoku;

      var iipeikou = iipeikouValues[0] + iipeikouValues[1] + iipeikouValues[2] + iipeikouValues[3];

      var honorSum = HonorSum(honorConcealedIndex, honorMeldIndex);
      
      var result = 0L;
      
      result |= waitAndWindShift & WaitShiftYakuFilter;
      
      result |= waitAndRonShift & WaitAndRonShiftYakuFilter;
      result += tankiBit * suuankouBit;
      
      result |= sanshoku & SanshokuYakuFilter;
      
      var x = iipeikou & ~((iipeikou & IipeikouYakuFilter) >> IipeikouDelta);
      result |= x & IipeikouYakuFilter;
      
      result |= honorSum & YakuhaiYakuFilter & valueWindFilter;

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

      return result;
    }

    private const long SanshokuYakuFilter = (0b1L << BitIndex.ClosedSanshokuDoujun) | (0b1L << BitIndex.OpenSanshokuDoujun) | (0b1L << BitIndex.SanshokuDoukou);

    private const long YakuhaiYakuFilter = (0b1L << BitIndex.Haku) | (0b1L << BitIndex.Hatsu) | (0b1L << BitIndex.Chun) |
                                           (0b1L << BitIndex.JikazeTon) | (0b1L << BitIndex.JikazeNan) | (0b1L << BitIndex.JikazeShaa) | (0b1L << BitIndex.JikazePei) |
                                           (0b1L << BitIndex.BakazeTon) | (0b1L << BitIndex.BakazeNan) | (0b1L << BitIndex.BakazeShaa) | (0b1L << BitIndex.BakazePei) |
                                           (0b1L << BitIndex.Shousangen) | (0b1L << BitIndex.Daisangen) | (0b1L << BitIndex.Shousuushi) | (0b1L << BitIndex.Daisuushi);

    private const long YakumanFilter = (0b1L << BitIndex.Daisangen) | (0b1L << BitIndex.Shousuushi) | (0b1L << BitIndex.Daisuushi);
    private const long ClosedYakuFilter = ~((0b1L << BitIndex.ClosedSanshokuDoujun));
    private const long OpenYakuFilter = ~((0b1L << BitIndex.OpenSanshokuDoujun));

    private const long IipeikouYakuFilter = (0b1L << IipeikouBitIndex) | (0b1L << ChiitoitsuBitIndex) | (0b1L << RyanpeikouBitIndex);
    private const long WaitAndRonShiftYakuFilter = (0b1L << SanankouBitIndex) | (0b1L << SuuankouBitIndex) | (0b1L << MenzenTsumoBitIndex);
    private const long WaitShiftYakuFilter = (0b1L << PinfuBitIndex) | (0b1L << JunseiChuurenPoutouBitIndex) | (0b1L << KokushiMusouJuusanMenBitIndex);
  }
}