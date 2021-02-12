// This file is licensed to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Spines.Mahjong.Analysis.Replay
{
  /// <summary>
  /// Creates walls and dice.
  /// </summary>
  internal class WallGenerator
  {
    /// <summary>
    /// Creates a new instance of WallGenerator.
    /// </summary>
    /// <param name="seed">The seed that is used to initialize the shuffler.</param>
    public WallGenerator(string seed)
    {
      _shuffler = new TenhouShuffler(CreateSeeds(seed));
    }

    /// <summary>
    /// Gets the dice for a game.
    /// </summary>
    /// <param name="gameIndex">The index of the game within the match.</param>
    /// <returns>A sequence of 2 dice.</returns>
    public IEnumerable<int> GetDice(int gameIndex)
    {
      while (_dice.Count <= gameIndex)
      {
        Generate();
      }
      return _dice[gameIndex];
    }

    /// <summary>
    /// Gets the wall of a game. The tiles 0 through 13 in the sequence form the dead wall.
    /// 5,7,9,11 are dora indicators, 4,6,8,10 are ura indicators.
    /// The dealer gets the last 4 tiles, the next player the second to last 4 tiles and so on.
    /// </summary>
    /// <param name="gameIndex">The index of the game within the match.</param>
    /// <returns>A sequence of 136 tiles.</returns>
    public IEnumerable<int> GetWall(int gameIndex)
    {
      while (_walls.Count <= gameIndex)
      {
        Generate();
      }
      return _walls[gameIndex];
    }

    private readonly IList<IEnumerable<int>> _dice = new List<IEnumerable<int>>();
    private readonly TenhouShuffler _shuffler;
    private readonly IList<IEnumerable<int>> _walls = new List<IEnumerable<int>>();

    /// <summary>
    /// Creates 9 chunks, then creates 9 hashes of 64 bytes each, which are converted into a total of 144 ints.
    /// </summary>
    /// <returns>144 random integers.</returns>
    private IEnumerable<int> Create144RandomValues()
    {
      // ToList to make sure that the shuffler is actually called 288 times and not lazily because GetNext modifies the shuffler's state.
      var values = Enumerable.Repeat(0, 288).Select(n => _shuffler.GetNext()).ToList();
      return CreateChunks(IntsToBytes(values), 128).SelectMany(ComputeHash);
    }

    private void Generate()
    {
      var rnd = Create144RandomValues().Select(v => unchecked((uint) v)).ToList();
      var wall = Enumerable.Range(0, 136).ToList();
      for (var i = 0; i < wall.Count - 1; ++i)
      {
        Swap(wall, i, i + Convert.ToInt16(rnd[i] % (136 - i)));
      }
      _walls.Add(wall);
      _dice.Add(new[] {CreateDice(rnd[135]), CreateDice(rnd[136])});
    }

    /// <summary>
    /// Converts n*4 bytes into n ints using Buffer.BlockCopy.
    /// </summary>
    /// <param name="source">The source bytes.</param>
    /// <returns>The resulting ints.</returns>
    private static IEnumerable<int> BytesToInts(IEnumerable<byte> source)
    {
      var array = source.ToArray();
      var numberOfInts = array.Length * sizeof(byte) / sizeof(int);
      var t = new int[numberOfInts];
      Buffer.BlockCopy(array, 0, t, 0, numberOfInts * sizeof(int));
      return t;
    }

    private static IEnumerable<int> ComputeHash(IEnumerable<byte> chunk)
    {
      using (var context = SHA512.Create())
      {
        return BytesToInts(context.ComputeHash(chunk.ToArray()));
      }
    }

    /// <summary>
    /// Converts the old style seed value (I've never even seen that one).
    /// </summary>
    /// <param name="parts">The original seed string split at each ',', skipping the first part of the split.</param>
    /// <returns>A sequence of ints.</returns>
    private static IEnumerable<int> ConvertOldSeed(IEnumerable<string> parts)
    {
      var prefixes = parts.Select(s => s.Split(new[] {'.'}, StringSplitOptions.None).First());
      return prefixes.Select(s => unchecked((int) Convert.ToUInt32(s, 16)));
    }

    private static IEnumerable<IEnumerable<T>> CreateChunks<T>(IEnumerable<T> source, int chunkSize)
    {
      var indexedValues = source.Select((s, i) => new {Value = s, Index = i});
      return indexedValues.GroupBy(item => item.Index / chunkSize, item => item.Value);
    }

    private static int CreateDice(uint randomValue)
    {
      return 1 + Convert.ToInt16(randomValue % 6);
    }

    private static IEnumerable<int> CreateSeeds(string seed)
    {
      var parts = seed.Split(new[] {','}, StringSplitOptions.None);
      if (parts.Length == 2 && parts[0] == "mt19937ar-sha512-n288-base64")
      {
        return BytesToInts(Convert.FromBase64String(parts[1]));
      }
      return ConvertOldSeed(parts.Skip(1));
    }

    /// <summary>
    /// Converts n ints into n*4 bytes using Buffer.BlockCopy.
    /// </summary>
    /// <param name="source">The source ints.</param>
    /// <returns>The resulting bytes.</returns>
    private static IEnumerable<byte> IntsToBytes(IEnumerable<int> source)
    {
      var array = source.ToArray();
      var numberOfBytes = array.Length * sizeof(int) / sizeof(byte);
      var t = new byte[numberOfBytes];
      Buffer.BlockCopy(array, 0, t, 0, numberOfBytes * sizeof(byte));
      return t;
    }

    private static void Swap<T>(IList<T> wall, int index1, int index2)
    {
      var t = wall[index1];
      wall[index1] = wall[index2];
      wall[index2] = t;
    }
  }
}