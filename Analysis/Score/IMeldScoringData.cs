using System.Collections.Generic;

namespace Spines.Mahjong.Analysis.Score
{
  internal interface IMeldScoringData
  {
    long BigAndToSumFilter { get; }

    long FinalMask { get; }

    IReadOnlyList<long> MeldLookupValues { get; }

    long OpenBit { get; }

    long SankantsuSuukantsu { get; }

    long ShiftedAnkanCount { get; }
  }
}