using System.Diagnostics;

namespace Spines.Mahjong.Analysis.Shanten
{
  /// <summary>
  /// Returns the arrangement value of honors after the execution of a single action.
  /// </summary>
  internal struct ProgressiveHonorClassifier
  {
    /*
     * actionIds:
     *
     * 0  draw with 0 of the same type in concealed hand
     * 1  draw with 1
     * 2  draw with 2
     * 3  draw with 3
     *
     * 4  draw with pon of the same type
     *
     * 5  discard with 1 of the same type in concealed hand
     * 6  discard with 2
     * 7  discard with 3
     * 8  discard with 4
     *
     * 9  discard with pon of the same type
     *
     * 10 pon with 2 of the same type in hand before pon
     * 11 pon with 3
     *
     * 12 daiminkan
     *
     * 13 shouminkan
     *
     * 14 ankan
     *
     * The next state is always at current + 1 + actionId
     */

    public ProgressiveHonorClassifier Clone()
    {
      return new ProgressiveHonorClassifier {_current = _current};
    }

    /// <summary>
    /// Draw a tile.
    /// </summary>
    /// <param name="previousTiles">Number of concealed tiles of that type before the draw.</param>
    /// <param name="meldBit">1 if there is a pon for that tileType, else 0.</param>
    /// <returns>Arrangement value of honors.</returns>
    public int Draw(int previousTiles, int meldBit)
    {
      var action = previousTiles + (meldBit << 2) + 1;
      _current = Transitions[_current + action];
      return Transitions[_current];
    }

    /// <summary>
    /// Discard a tile.
    /// </summary>
    /// <param name="tilesAfterDiscard">Number of concealed tiles of that type after the discard.</param>
    /// <param name="meldBit">1 if there is a pon for that tileType, else 0.</param>
    /// <returns>Arrangement value of honors.</returns>
    public int Discard(int tilesAfterDiscard, int meldBit)
    {
      var action = tilesAfterDiscard + (meldBit << 2) + 6;
      _current = Transitions[_current + action];
      return Transitions[_current];
    }

    public int Pon(int previousTiles)
    {
      Debug.Assert(previousTiles > 1, "Can't pon with less than 2 tiles in hand");
      _current = Transitions[_current + previousTiles + 9];
      return Transitions[_current];
    }

    public int Daiminkan()
    {
      _current = Transitions[_current + 13];
      return Transitions[_current];
    }

    public int Shouminkan()
    {
      _current = Transitions[_current + 14];
      return Transitions[_current];
    }

    public int Ankan()
    {
      _current = Transitions[_current + 15];
      return Transitions[_current];
    }

    private static readonly ushort[] Transitions = Resource.Transitions("ProgressiveHonorStateMachine.txt");

    private int _current;
  }
}