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
    }

    public static long Flags(HandCalculator hand)
    {
      var manzuConcealedIndex = hand.Base5Hash(0);
      var manzuMeldIndex = MeldIndex(hand, 0);
      var pinzuConcealedIndex = hand.Base5Hash(1);
      var pinzuMeldIndex = MeldIndex(hand, 1);
      var souzuConcealedIndex = hand.Base5Hash(2);
      var souzuMeldInex = MeldIndex(hand, 2);
      var honorConcealedIndex = hand.Base5Hash(3);
      var honorMeldIndex = MeldIndex(hand, 3);

      var manzuAnd = SuitAnd(manzuConcealedIndex, manzuMeldIndex);
      var pinzuAnd = SuitAnd(pinzuConcealedIndex, pinzuMeldIndex);
      var souzuAnd = SuitAnd(souzuConcealedIndex, souzuMeldInex);
      var honorAnd = HonorAnd(honorConcealedIndex, honorMeldIndex);

      var andField = manzuAnd & souzuAnd & pinzuAnd & honorAnd;

      //var manzuSum = SuitSum(manzuConcealedIndex, manzuMeldIndex);
      //var pinzuSum = SuitSum(pinzuConcealedIndex, pinzuMeldIndex);
      //var souzuSum = SuitSum(souzuConcealedIndex, souzuMeldInex);
      var honorSum = HonorSum(honorConcealedIndex, honorMeldIndex);
      
      var sangenSuushi = honorSum & (0b100100100100L << 47);

      return andField | sangenSuushi;
    }

    private static long HonorSum(int concealedIndex, int meldIndex)
    {
      var concealedSum = HonorConcealed[concealedIndex + HonorConcealedBlockSize];
      var meldSum = HonorMelded[meldIndex + 2 * MeldBlockSize];

      var sum = concealedSum + meldSum;
      return sum;
    }

    private const int SuitConcealedBlockSize = 1953125;
    private const int MeldBlockSize = 456976;
    private const int HonorConcealedBlockSize = 78125;

    private static readonly long[] HonorConcealed;
    private static readonly long[] HonorMelded;
    private static readonly long[] SuitConcealed;
    private static readonly long[] SuitMelded;

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
        meldIndex *= 26;
        meldIndex += i + 1;
      }

      return meldIndex;
    }
  }
}