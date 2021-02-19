using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AnalyzerBuilder.Combinations
{
  /// <summary>
  /// Creates possible combinations of tiles used in melds in one suit.
  /// </summary>
  internal class MeldedCombinationsCreator : CombinationCreatorBase
  {
    /// <summary>
    /// Creates a MeldedCombinationsCreator for a suit.
    /// </summary>
    /// <returns>An instance of MeldedCombinationsCreator.</returns>
    public static MeldedCombinationsCreator ForSuits()
    {
      return new MeldedCombinationsCreator(9, Mentsu.All);
    }

    /// <summary>
    /// Creates a MeldedCombinationsCreator for honors.
    /// </summary>
    /// <returns>An instance of MeldedCombinationsCreator.</returns>
    public static MeldedCombinationsCreator ForHonors()
    {
      return new MeldedCombinationsCreator(7, new[] {Mentsu.Kantsu, Mentsu.Koutsu});
    }

    /// <summary>
    /// Creates all possible combinations of used tiles for a number of melds in a single suit.
    /// </summary>
    public IEnumerable<Combination> Create(int numberOfMelds)
    {
      Debug.Assert(numberOfMelds >= 0);
      Clear();
      return Create(numberOfMelds, 0);
    }

    /// <summary>
    /// Creates a new instance of MeldedCombinationsCreator.
    /// </summary>
    private MeldedCombinationsCreator(int tileTypes, IEnumerable<Mentsu> mentsu)
      : base(tileTypes)
    {
      _mentsu = mentsu;
    }

    private readonly IEnumerable<Mentsu> _mentsu;

    /// <summary>
    /// Creates all possible combinations of used tiles for a number of melds in a single suit.
    /// </summary>
    private IEnumerable<Combination> Create(int remainingMelds, int currentIndex)
    {
      // All melds used, return the current used tiles.
      if (remainingMelds == 0)
      {
        return CreateCurrentCombination().Yield();
      }

      return _mentsu.SelectMany(m => Create(remainingMelds, currentIndex, m));
    }

    /// <summary>
    /// Can a meld be added to the current accumulator?
    /// </summary>
    private bool CanAddMeld(int index, Mentsu mentsu)
    {
      if (index > TypesInSuit - mentsu.Stride)
      {
        return false;
      }
      var max = TilesPerType - mentsu.Amount;
      return Accumulator.Skip(index).Take(mentsu.Stride).All(i => i <= max);
    }

    /// <summary>
    /// Creates all possible combinations of used tiles for a number of melds in a single suit by adding a specific meld.
    /// </summary>
    private IEnumerable<Combination> Create(int remainingMelds, int currentIndex, Mentsu mentsu)
    {
      var indices = Enumerable.Range(currentIndex, TypesInSuit - currentIndex);
      var freeIndices = indices.Where(i => CanAddMeld(i, mentsu));
      foreach (var index in freeIndices)
      {
        AddToAccumulator(index, mentsu.Stride, mentsu.Amount);
        foreach (var combination in Create(remainingMelds - 1, index))
        {
          yield return combination;
        }
        AddToAccumulator(index, mentsu.Stride, -mentsu.Amount);
      }
    }

    /// <summary>
    /// Adds an amount to multiple entries in the accumulator.
    /// </summary>
    /// <param name="index">The index of the first entry to adjust.</param>
    /// <param name="stride">The number of entries to adjust.</param>
    /// <param name="amount">The amount that is added to each entry.</param>
    private void AddToAccumulator(int index, int stride, int amount)
    {
      for (var i = 0; i < stride; ++i)
      {
        Accumulator[index + i] += amount;
      }
    }
  }
}