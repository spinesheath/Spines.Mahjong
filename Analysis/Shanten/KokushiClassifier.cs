namespace Spines.Mahjong.Analysis.Shanten
{
  /// <summary>
  /// Progressively calculates the Shanten for Kokushi.
  /// It is expected to only be called for changes to terminal or honor tiles.
  /// It only matters how many tiles of a tile type are in the hand, but not which tile types those are (aside from them being terminals or honors).
  /// Therefore when drawing or discarding the only additional input is how many tiles were in the hand before that action.
  /// </summary>
  internal struct KokushiClassifier
  {
    public static KokushiClassifier Create()
    {
      return new KokushiClassifier(14, 1);
    }

    public KokushiClassifier Clone()
    {
      return new KokushiClassifier(Shanten, _pairs);
    }

    /// <summary>
    /// Shanten + 1 because in Hand calculations are done with that value instead of real shanten.
    /// </summary>
    public int Shanten;
    
    public void Draw(int kyuuhaiValue, int previousTileCount)
    {
      // TODO I suspect this can be simplified

      // 1 if previousTileCount < 2, else 0
      var s = (previousTileCount ^ 2) >> 1 & kyuuhaiValue;
      // 1 if previousTileCount == 1, else 0
      var p = previousTileCount & s;
      // 1 if no pair was added or there were no pairs before, else 0
      var t = (_pairs | ~p) & s;
      _pairs <<= p;
      Shanten -= t;
    }

    public void Discard(int kyuuhaiValue, int tileCountAfterDiscard)
    {
      // TODO I suspect this can be simplified

      // 1 if tileCountAfterDiscard < 2, else 0
      var s = (tileCountAfterDiscard ^ 2) >> 1 & kyuuhaiValue;
      // 1 if tileCountAfterDiscard == 1, else 0
      var p = tileCountAfterDiscard & s;
      _pairs >>= p;
      // 1 if no pair was removed or there were at least two pairs before, else 0
      var t = (_pairs | ~p) & s;
      Shanten += t;
    }

    private int _pairs;

    private KokushiClassifier(int shanten, int pairs)
    {
      Shanten = shanten;
      _pairs = pairs;
    }
  }
}