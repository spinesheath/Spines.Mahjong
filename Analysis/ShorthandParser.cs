using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis
{
  /// <summary>
  /// Parses the shorthand representation of a hand.
  /// </summary>
  public class ShorthandParser
  {
    public ShorthandParser(string hand)
    {
      _hand = hand;
    }

    public IEnumerable<Tile> Tiles
    {
      get
      {
        var concealed = Concealed.ToList();
        for (var i = 0; i < 34; ++i)
        {
          for (var c = 0; c < concealed[i]; ++c)
          {
            yield return new Tile {Suit = IdToSuit[i / 9], Index = i % 9};
          }
        }
      }
    }

    public IEnumerable<Meld> Melds
    {
      get
      {
        var manzu = ManzuMeldIds.Select(meldId => new Meld(Suit.Manzu, meldId));
        var pinzu = PinzuMeldIds.Select(meldId => new Meld(Suit.Pinzu, meldId));
        var souzu = SouzuMeldIds.Select(meldId => new Meld(Suit.Souzu, meldId));
        var jihai = JihaiMeldIds.Select(meldId => new Meld(Suit.Jihai, meldId));
        return manzu.Concat(pinzu).Concat(souzu).Concat(jihai);
      }
    }

    private static readonly Suit[] IdToSuit = {Suit.Manzu, Suit.Pinzu, Suit.Souzu, Suit.Jihai};

    private readonly string _hand;

    private IEnumerable<int> GetMelds(char tileGroupName, char[] forbidden)
    {
      var regex = new Regex(@"(\d*)" + tileGroupName);
      var groups = regex.Matches(_hand).SelectMany(m => m.Groups.OfType<Group>().Skip(1));
      foreach (var captureGroup in groups)
      {
        if (forbidden.Intersect(captureGroup.Value).Any())
        {
          throw ForbiddenDigitsException(tileGroupName, forbidden);
        }
        var tiles = captureGroup.Value.Select(GetTileTypeIndex).OrderBy(x => x).ToList();
        var i = tiles.Min();
        if (tiles.SequenceEqual(Enumerable.Range(i, 3)))
        {
          yield return i;
        }
        else if (tiles.SequenceEqual(Enumerable.Repeat(i, 3)))
        {
          yield return 7 + i;
        }
        else if (tiles.SequenceEqual(Enumerable.Repeat(i, 4)))
        {
          yield return 7 + 9 + i;
        }
        else
        {
          throw new FormatException(captureGroup.Value + " is not a valid meld.");
        }
      }
    }

    private IEnumerable<int> GetTiles(char tileGroupName, int typesInSuit, char[] forbidden)
    {
      var regex = new Regex(@"(\d*)" + tileGroupName);
      var groups = regex.Matches(_hand).SelectMany(m => m.Groups.OfType<Group>().Skip(1));
      var digits = groups.SelectMany(g => g.Value).ToList();
      if (digits.Intersect(forbidden).Any())
      {
        throw ForbiddenDigitsException(tileGroupName, forbidden);
      }
      var tiles = digits.Select(GetTileTypeIndex);
      var idToCount = tiles.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());
      return Enumerable.Range(0, typesInSuit).Select(i => idToCount.ContainsKey(i) ? idToCount[i] : 0);
    }

    private static FormatException ForbiddenDigitsException(char tileGroupName, char[] forbidden)
    {
      return new FormatException(string.Join(",", forbidden) + " are not allowed in group " + tileGroupName + ".");
    }

    /// <summary>
    /// Returns the index of a tile type within a suit.
    /// </summary>
    /// <param name="digit">The digit that represents the tile in shorthand notation.</param>
    /// <returns>The index of the tile type.</returns>
    private static int GetTileTypeIndex(char digit)
    {
      var numericValue = (int) char.GetNumericValue(digit);
      if (numericValue == 0)
      {
        return 4;
      }
      return numericValue - 1;
    }

    /// <summary>
    /// The counts of the 34 tile types, in order manzu 1-9, pinzu 1-9, souzu 1-9, honors 1-7.
    /// </summary>
    internal IEnumerable<int> Concealed => Manzu.Concat(Pinzu).Concat(Souzu).Concat(Jihai);

    /// <summary>
    /// The counts of the 9 manzu types, in order 1-9.
    /// </summary>
    internal IEnumerable<int> Manzu => GetTiles('m', 9, new char[0]);

    /// <summary>
    /// The counts of the 9 pinzu types, in order 1-9.
    /// </summary>
    internal IEnumerable<int> Pinzu => GetTiles('p', 9, new char[0]);

    /// <summary>
    /// The counts of the 9 souzu types, in order 1-9.
    /// </summary>
    internal IEnumerable<int> Souzu => GetTiles('s', 9, new char[0]);

    /// <summary>
    /// The counts of the 7 honor types, in order 1-7.
    /// </summary>
    internal IEnumerable<int> Jihai => GetTiles('z', 7, new[] {'0', '8', '9'});

    internal IEnumerable<int> ManzuMeldIds => GetMelds('M', new char[0]);

    internal IEnumerable<int> PinzuMeldIds => GetMelds('P', new char[0]);

    internal IEnumerable<int> SouzuMeldIds => GetMelds('S', new char[0]);

    internal IEnumerable<int> JihaiMeldIds => GetMelds('Z', new[] {'0', '8', '9'});
  }
}