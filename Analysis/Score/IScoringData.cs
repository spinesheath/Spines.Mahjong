using System.Collections.Generic;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Score
{
  internal interface IScoringData
  {
    long BigAndToSumFilter { get; }

    long FinalMask { get; }

    IReadOnlyList<long> MeldLookupValues { get; }

    long OpenBit { get; }

    long SankantsuSuukantsu { get; }

    long ShiftedAnkanCount { get; }

    long[] WaitShiftValues { get; }
    
    long[] SuitOr { get; }

    long HonorOr { get; }

    long HonorSum { get; }
  }
}