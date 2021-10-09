namespace Spines.Mahjong.Analysis.Score
{
  internal interface IScoringData
  {
    long BigAndToSumFilter { get; }

    long FinalMask { get; }

    long OpenBit { get; }

    long SankantsuSuukantsu { get; }

    long ShiftedAnkanCount { get; }

    long[] WaitShiftValues { get; }
    
    long[] SuitOr { get; }

    long HonorOr { get; }

    long HonorSum { get; }

    int Fu { get; }
  }
}