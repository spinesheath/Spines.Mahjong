using System.Collections.Generic;

namespace AnalyzerBuilder.Combinations
{
  internal class ShuntsuProtoGroupInserter : IProtoGroupInserter
  {
    public ShuntsuProtoGroupInserter(int requiredLeft, int requiredMiddle, int requiredRight)
    {
      _requiredLeft = requiredLeft;
      _requiredMiddle = requiredMiddle;
      _requiredRight = requiredRight;
    }

    public bool CanInsert(IReadOnlyList<int> concealedTiles, IReadOnlyList<int> usedTiles, int offset)
    {
      if (offset > 6)
      {
        return false;
      }
      if (usedTiles[offset + 0] == 4 || usedTiles[offset + 1] == 4 || usedTiles[offset + 2] == 4)
      {
        return false;
      }
      return AreBothZeroOrNeither(concealedTiles[offset + 0], _requiredLeft) &&
             AreBothZeroOrNeither(concealedTiles[offset + 1], _requiredMiddle) &&
             AreBothZeroOrNeither(concealedTiles[offset + 2], _requiredRight);
    }

    public void Insert(IList<int> concealedTiles, IList<int> usedTiles, int offset)
    {
      concealedTiles[offset + 0] -= _requiredLeft;
      concealedTiles[offset + 1] -= _requiredMiddle;
      concealedTiles[offset + 2] -= _requiredRight;
      usedTiles[offset + 0] += 1;
      usedTiles[offset + 1] += 1;
      usedTiles[offset + 2] += 1;
    }

    public void Remove(IList<int> concealedTiles, IList<int> usedTiles, int offset)
    {
      concealedTiles[offset + 0] += _requiredLeft;
      concealedTiles[offset + 1] += _requiredMiddle;
      concealedTiles[offset + 2] += _requiredRight;
      usedTiles[offset + 0] -= 1;
      usedTiles[offset + 1] -= 1;
      usedTiles[offset + 2] -= 1;
    }

    private readonly int _requiredLeft;
    private readonly int _requiredMiddle;
    private readonly int _requiredRight;

    /// <summary>
    /// True if either both values are 0 or both values are not 0.
    /// </summary>
    private static bool AreBothZeroOrNeither(int lhs, int rhs)
    {
      var concealedZero = lhs == 0;
      var requiredZero = rhs == 0;
      return concealedZero == requiredZero;
    }
  }
}