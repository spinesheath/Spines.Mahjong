using System.Collections.Generic;
using Spines.Mahjong.Analysis.Resources;

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

    private static readonly long[] HonorConcealed;
    private static readonly long[] HonorMelded;
    private static readonly long[] SuitConcealed;
    private static readonly long[] SuitMelded;

    public static long Suit(IReadOnlyList<int> concealedTileCounts, IReadOnlyList<int> meldIds)
    {
      var concealedIndex = 0;
      foreach (var c in concealedTileCounts)
      {
        concealedIndex *= 5;
        concealedIndex += c;
      }

      var concealedAnd = SuitConcealed[concealedIndex];

      var meldIndex = 0;
      foreach (var i in meldIds)
      {
        meldIndex *= 26;
        meldIndex += i + 1;
      }

      var meldAnd = SuitMelded[meldIndex];
      var meldOr = SuitMelded[meldIndex + 456976];

      var result = (concealedAnd & meldAnd) | meldOr;
      return result;
    }

    public static long Honor(IReadOnlyList<int> concealedTileCounts, IReadOnlyList<int> meldIds)
    {
      var concealedIndex = 0;
      foreach (var c in concealedTileCounts)
      {
        concealedIndex *= 5;
        concealedIndex += c;
      }

      var concealedAnd = HonorConcealed[concealedIndex];

      var meldIndex = 0;
      foreach (var i in meldIds)
      {
        meldIndex *= 26;
        meldIndex += i + 1;
      }

      var meldAnd = HonorMelded[meldIndex];
      var meldOr = HonorMelded[meldIndex + 456976];

      var result = (concealedAnd & meldAnd) | meldOr;
      return result;
    }
  }
}