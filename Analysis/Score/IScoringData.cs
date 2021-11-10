namespace Spines.Mahjong.Analysis.Score
{
  internal interface IScoringData
  {
    long BigAndToSumFilter { get; }

    long FinalMask { get; }

    int Fu { get; }

    long HonorOr { get; }

    long HonorSum { get; }

    long OpenBit { get; }

    long SankantsuSuukantsu { get; }

    long ShiftedAnkanCount { get; }

    long[] SuitOr { get; }

    long[] WaitShiftValues { get; }

    int FuFootprint(int suitId, int index);
  }
}