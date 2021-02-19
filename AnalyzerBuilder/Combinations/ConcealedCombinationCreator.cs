using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AnalyzerBuilder.Combinations
{
  /// <summary>
  /// Creates possible combinations of tiles in one suit.
  /// </summary>
  internal class ConcealedCombinationCreator : CombinationCreatorBase
  {
    /// <summary>
    /// Creates a ConcealedCombinationCreator for a suit.
    /// </summary>
    /// <returns>An instance of ConcealedCombinationCreator.</returns>
    public static ConcealedCombinationCreator ForSuits()
    {
      return new ConcealedCombinationCreator(9);
    }

    /// <summary>
    /// Creates a ConcealedCombinationCreator for honors.
    /// </summary>
    /// <returns>An instance of ConcealedCombinationCreator.</returns>
    public static ConcealedCombinationCreator ForHonors()
    {
      return new ConcealedCombinationCreator(7);
    }

    /// <summary>
    /// Creates all possible semantically unique concealed combinations for a given number of tiles.
    /// </summary>
    public IEnumerable<Combination> Create(int numberOfTiles)
    {
      var noMeldedTiles = new Combination(new int[TypesInSuit]);
      return Create(numberOfTiles, noMeldedTiles);
    }

    /// <summary>
    /// Creates all possible semantically unique concealed combinations for a given number of tiles and a set of tiles already
    /// used in melds.
    /// </summary>
    /// <param name="numberOfTiles">The number of tiles in the concealed part of the hand.</param>
    /// <param name="meldedTiles">The tiles used in the melded part of the hand.</param>
    public IEnumerable<Combination> Create(int numberOfTiles, Combination meldedTiles)
    {
      Debug.Assert(numberOfTiles >= 0);
      Debug.Assert(meldedTiles != null);
      Clear();
      var used = meldedTiles.Counts.ToList();
      for (var i = 0; i < TypesInSuit; ++i)
      {
        TilesInExternalMelds[i] = used[i];
      }
      return Create(numberOfTiles, TypesInSuit);
    }

    /// <summary>
    /// Creates a new instance of ConcealedCombinationCreator.
    /// </summary>
    private ConcealedCombinationCreator(int tileTypes)
      : base(tileTypes)
    {
    }

    /// <summary>
    /// Recursively creates possible concealed combinations in one suit.
    /// </summary>
    private IEnumerable<Combination> Create(int remainingTiles, int remainingTypes)
    {
      // If all types have been tried we are done.
      if (remainingTypes == 0)
      {
        var weight = GetWeight();
        if (remainingTiles == 0 && weight >= 0)
        {
          yield return CreateCurrentCombination();
        }
      }
      else
      {
        var index = TypesInSuit - remainingTypes;
        var freeTiles = TilesPerType - TilesInExternalMelds[index];
        // The maximum amount of tiles that can be used for the current type.
        var max = Math.Min(remainingTiles, freeTiles);
        // Add 0 to max tiles of the current type and accumulate results.
        for (var i = 0; i <= max; ++i)
        {
          Accumulator[index] = i;
          foreach (var gd in Create(remainingTiles - i, remainingTypes - 1))
          {
            yield return gd;
          }
        }
      }
    }
  }
}