using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Combinations
{
  /// <summary>
  /// Base class for combination creators.
  /// </summary>
  internal abstract class CombinationCreatorBase
  {
    /// <summary>
    /// Creates a new instance of CombinationCreatorBase.
    /// </summary>
    protected CombinationCreatorBase(int typesInSuit)
    {
      TypesInSuit = typesInSuit;
    }

    /// <summary>
    /// There are 4 tiles of each type.
    /// </summary>
    protected const int TilesPerType = 4;

    /// <summary>
    /// 9 for a suit or 7 for honors.
    /// </summary>
    protected int TypesInSuit { get; }

    /// <summary>
    /// Represents the number of tiles in the suit.
    /// </summary>
    protected IList<int> Accumulator { get; private set; }

    /// <summary>
    /// Tiles that are used in melds.
    /// </summary>
    protected IList<int> TilesInExternalMelds { get; private set; }

    /// <summary>
    /// Resets the accumulator.
    /// </summary>
    protected void Clear()
    {
      Accumulator = new int[TypesInSuit];
      TilesInExternalMelds = new int[TypesInSuit];
    }

    /// <summary>
    /// Creates a new combination from the current state of the accumulator.
    /// </summary>
    protected Combination CreateCurrentCombination()
    {
      return new Combination(Accumulator.ToList());
    }

    /// <summary>
    /// Calculates the combined weight of all tiles.
    /// The weight of a combination balanced around the middle is 0.
    /// Tiles to the left have positive weight, tiles to the right have negative weight.
    /// </summary>
    protected int GetWeight()
    {
      return Enumerable.Range(0, TypesInSuit).Sum(GetWeight);
    }

    /// <summary>
    /// Calculates the weight of a single tile type and count.
    /// TileTypes to the left have positive weight, to the right have negative.
    /// </summary>
    private int GetWeight(int tileTypeIndex)
    {
      var tileCount = Accumulator[tileTypeIndex] + TilesInExternalMelds[tileTypeIndex];
      var centerIndex = (TypesInSuit - TypesInSuit % 1) / 2;
      var shift = Math.Abs(centerIndex - tileTypeIndex) * 2;
      var factor = Math.Sign(centerIndex - tileTypeIndex);
      return (1 << shift) * tileCount * factor;
    }
  }
}