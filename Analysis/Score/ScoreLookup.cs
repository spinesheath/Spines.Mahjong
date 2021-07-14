using Spines.Mahjong.Analysis.Resources;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Score
{
  internal static class ScoreLookup
  {
    static ScoreLookup()
    {
      SuitConcealed = Resource.LongLookup("Scoring", "SuitScoringLookup.dat");
      SuitMelded = Resource.LongLookup("Scoring", "SuitMeldScoringLookup.dat");
      HonorConcealed = Resource.LongLookup("Scoring", "HonorScoringLookup.dat");
      HonorMelded = Resource.LongLookup("Scoring", "HonorMeldScoringLookup.dat");

      HonorSumLookup = Resource.LongLookup("Scoring", "HonorSumLookup.dat");
      HonorMeldSumLookup = Resource.LongLookup("Scoring", "HonorMeldSumLookup.dat");
    }

    public static long Flags(HandCalculator hand, Tile winningTile, bool isRon, int valueWindFlags)
    {
      var manzuConcealedIndex = hand.Base5Hash(0);
      var manzuMeldIndex = MeldIndex(hand, 0);
      var pinzuConcealedIndex = hand.Base5Hash(1);
      var pinzuMeldIndex = MeldIndex(hand, 1);
      var souzuConcealedIndex = hand.Base5Hash(2);
      var souzuMeldIndex = MeldIndex(hand, 2);
      var honorConcealedIndex = hand.Base5Hash(3);
      var honorMeldIndex = MeldIndex(hand, 3);

      var manzuAnd = SuitAnd(manzuConcealedIndex, manzuMeldIndex);
      var pinzuAnd = SuitAnd(pinzuConcealedIndex, pinzuMeldIndex);
      var souzuAnd = SuitAnd(souzuConcealedIndex, souzuMeldIndex);
      var honorAnd = HonorAnd(honorConcealedIndex, honorMeldIndex);

      var andField = manzuAnd & souzuAnd & pinzuAnd & honorAnd;

      var manzuSum = SuitSum(manzuConcealedIndex, manzuMeldIndex);
      var pinzuSum = SuitSum(pinzuConcealedIndex, pinzuMeldIndex);
      var souzuSum = SuitSum(souzuConcealedIndex, souzuMeldIndex);
      var suitSum = manzuSum + pinzuSum + souzuSum;

      var honorSum = HonorSum(honorConcealedIndex, honorMeldIndex);

      var sangenSuushi = honorSum & (0b100100100100L << 47);

      var ittsuu = ((manzuAnd | pinzuAnd | souzuAnd) + (0b1L << 59)) & (0b1L << 62);

      var shiftValues = new int[4];
      shiftValues[winningTile.TileType.SuitId] = winningTile.TileType.Index + (isRon ? 10 : 1);

      var manzuShift = SuitShift(manzuConcealedIndex) >> shiftValues[0];
      var pinzuShift = SuitShift(pinzuConcealedIndex) >> shiftValues[1];
      var souzuShift = SuitShift(souzuConcealedIndex) >> shiftValues[2];

      // TODO pinfu 1 bit from honors too, add instead of AND, bitmask 100
      // TODO ankou SUM + (SUM & 101)
      var pinfu = manzuShift & pinzuShift & souzuShift & 0b1L;

      return andField | sangenSuushi | suitSum | ittsuu;
    }

    private static long SuitShift(int concealedIndex)
    {
      return SuitConcealed[concealedIndex + 2 * SuitConcealedBlockSize];
    }

    private static long HonorSum(int concealedIndex, int meldIndex)
    {
      var concealedSum = HonorSumLookup[concealedIndex];
      var meldSum = HonorMeldSumLookup[meldIndex];

      var sum = concealedSum + meldSum;
      return sum;
    }

    private const int SuitConcealedBlockSize = 1953125;
    private const int MeldBlockSize = 1500625;
    private const int HonorConcealedBlockSize = 78125;

    private static readonly long[] HonorConcealed;
    private static readonly long[] HonorMelded;
    private static readonly long[] HonorSumLookup;
    private static readonly long[] HonorMeldSumLookup;
    private static readonly long[] SuitConcealed;
    private static readonly long[] SuitMelded;

    private const int PinfuBitIndex = 10;
    private const int KokushiMusouJuusanMenBitIndex = 0;
    private const int JunseiChuurenPoutouBitIndex = 22;

    private const int SanankouBitIndex = 34;
    private const int AnkouRonShiftSumFilterIndex = SanankouBitIndex - 2;
    private const int SuuankouBitIndex = SanankouBitIndex + 1;
    private const int SuuankouTankiBitIndex = SuuankouBitIndex + 1;

    private const int MenzenTsumoBitIndex = 56;
    private const int MenzenTsumoRonShiftSumFilterIndex = MenzenTsumoBitIndex - 2;

    private const int ClosedSanshokuDoujunBitIndex = 2;
    private const int OpenSanshokuDoujunBitIndex = 3;
    private const int SanshokuDoukouBitIndex = 4;

    private const int IipeikouDelta = 4;
    private const int IipeikouBitIndex = 37;
    private const int ChiitoitsuBitIndex = IipeikouBitIndex + IipeikouDelta;
    private const int RyanpeikouBitIndex = ChiitoitsuBitIndex + IipeikouDelta;

    private static long SuitSum(int concealedIndex, int meldIndex)
    {
      var concealedSum = SuitConcealed[concealedIndex + SuitConcealedBlockSize];
      var meldSum = SuitMelded[meldIndex + 2 * MeldBlockSize];

      var sum = concealedSum + meldSum;
      return sum;
    }

    private static long SuitAnd(int concealedIndex, int meldIndex)
    {
      var concealedAnd = SuitConcealed[concealedIndex];
      var meldAnd = SuitMelded[meldIndex];
      var meldOr = SuitMelded[meldIndex + MeldBlockSize];

      var result = (concealedAnd & meldAnd) | meldOr;
      return result;
    }

    private static long HonorAnd(int concealedIndex, int meldIndex)
    {
      var concealedAnd = HonorConcealed[concealedIndex];
      var meldAnd = HonorMelded[meldIndex];
      var meldOr = HonorMelded[meldIndex + MeldBlockSize];

      var result = (concealedAnd & meldAnd) | meldOr;
      return result;
    }

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
    public static long Flags2(HandCalculator hand, Tile winningTile, bool isRon, int roundWind, int seatWind)
    {
      var manzuConcealedIndex = hand.Base5Hash(0);
      var manzuMeldIndex = MeldIndex(hand, 0);
      var pinzuConcealedIndex = hand.Base5Hash(1);
      var pinzuMeldIndex = MeldIndex(hand, 1);
      var souzuConcealedIndex = hand.Base5Hash(2);
      var souzuMeldIndex = MeldIndex(hand, 2);
      var honorConcealedIndex = hand.Base5Hash(3);
      var honorMeldIndex = MeldIndex(hand, 3);
      
      
      var shiftedAnkanCount = 0b0L;
      var waitShiftValues = new [] {1L, 1L, 1L, 1L};
      var concealedOrMeldedValues = new[] { 1L, 1L, 1L };
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

      // TODO valueWindFlags has both winds in 4 bits for pinfu, but for yakuhai purposes the winds need to be 1 bit in 4 bits twice
      result |= honorSum & YakuhaiYakuFilter & valueWindFilter;

      var yakuman = result & YakumanFilter;

      return yakuman != 0 ? yakuman : result;
    }

    private const long YakuhaiYakuFilter = (0b1L << BitIndex.Haku) | (0b1L << BitIndex.Hatsu) | (0b1L << BitIndex.Chun) |
                                           (0b1L << BitIndex.JikazeTon) | (0b1L << BitIndex.JikazeNan) | (0b1L << BitIndex.JikazeShaa) | (0b1L << BitIndex.JikazePei) |
                                           (0b1L << BitIndex.BakazeTon) | (0b1L << BitIndex.BakazeNan) | (0b1L << BitIndex.BakazeShaa) | (0b1L << BitIndex.BakazePei) |
                                           (0b1L << BitIndex.Shousangen) | (0b1L << BitIndex.Daisangen) | (0b1L << BitIndex.Shousuushi) | (0b1L << BitIndex.Daisuushi);

    private const long YakumanFilter = (0b1L << BitIndex.Daisangen) | (0b1L << BitIndex.Shousuushi) | (0b1L << BitIndex.Daisuushi);

    private const long IipeikouYakuFilter = (0b1L << IipeikouBitIndex) | (0b1L << ChiitoitsuBitIndex) | (0b1L << RyanpeikouBitIndex);
    private const long SanshokuYakuFilter = (0b1L << ClosedSanshokuDoujunBitIndex) | (0b1L << OpenSanshokuDoujunBitIndex) | (0b1L << SanshokuDoukouBitIndex);
    private const long WaitAndRonShiftYakuFilter = (0b1L << SanankouBitIndex) | (0b1L << SuuankouBitIndex) | (0b1L << MenzenTsumoBitIndex);
    private const long WaitShiftYakuFilter = (0b1L << PinfuBitIndex) | (0b1L << JunseiChuurenPoutouBitIndex) | (0b1L << KokushiMusouJuusanMenBitIndex);
  }
}