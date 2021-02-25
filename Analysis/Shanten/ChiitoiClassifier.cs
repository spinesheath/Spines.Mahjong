namespace Spines.Mahjong.Analysis.Shanten
{
  /// <summary>
  /// Progressively calculates the chiitoitsu shanten of a hand.
  /// It only matters how many tiles of a tile type are in the hand, but not which tile types those are.
  /// Therefore when drawing or discarding the only additional input is how many tiles were in the hand before that action.
  /// </summary>
  internal struct ChiitoiClassifier
  {
    public static ChiitoiClassifier Create()
    {
      return new ChiitoiClassifier(7);
    }

    public ChiitoiClassifier Clone()
    {
      return new ChiitoiClassifier(Shanten);
    }

    /// <summary>
    /// Shanten + 1 because in Hand calculations are done with that value instead of real shanten.
    /// </summary>
    public int Shanten;

    public void Draw(int previousTileCount)
    {
      // ((x >> 1) ^ 001) & x
      // 1 if x == 1 else 0
      Shanten -= ((previousTileCount >> 1) ^ 1) & previousTileCount;
    }

    public void Discard(int previousTileCount)
    {
      // ((x >> 1) ^ 001) & x
      // 1 if x == 1 else 0
      var x = previousTileCount - 1;
      Shanten += ((x >> 1) ^ 1) & x;
    }

    private ChiitoiClassifier(int shanten)
    {
      Shanten = shanten;
    }
  }
}