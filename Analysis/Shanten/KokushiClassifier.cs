using System.Runtime.CompilerServices;

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
      return new KokushiClassifier(14, 0);
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
      var s = kyuuhaiValue >> (_pairs * previousTileCount);
      Shanten -= s;
      var p = (2 >> previousTileCount) & kyuuhaiValue;
      _pairs += p;
    }
    
    public void Discard(int kyuuhaiValue, int tileCountAfterDiscard)
    {
      var s = (3 >> (_pairs * tileCountAfterDiscard)) & kyuuhaiValue;
      Shanten += s;
      var p = (2 >> tileCountAfterDiscard) & kyuuhaiValue;
      _pairs -= p;
    }

    private int _pairs;

    private KokushiClassifier(int shanten, int pairs)
    {
      Shanten = shanten;
      _pairs = pairs;
    }
  }
}