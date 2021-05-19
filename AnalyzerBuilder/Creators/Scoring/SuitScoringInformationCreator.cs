using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class SuitScoringInformationCreator
  {
    public SuitScoringInformationCreator(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    public void CreateLookup()
    {
      const int maxLookupIndex = 1953125;
      var lookup = new long[maxLookupIndex * 3];

      var language = CreateAnalyzedWords();
      var groupedByHash = language.GroupBy(w => w.Base5Hash);
      foreach (var group in groupedByHash)
      {
        var index = group.Key;
        var field = new SuitScoringBitField(group);

        Debug.Assert(lookup[index] == 0 || lookup[index] == field.AndValue);
        Debug.Assert(lookup[index + maxLookupIndex] == 0 || lookup[index + maxLookupIndex] == field.SumValue);
        Debug.Assert(lookup[index + 2 * maxLookupIndex] == 0 || lookup[index + 2 * maxLookupIndex] == field.WaitShiftValue);

        lookup[index] = field.AndValue;
        lookup[index + maxLookupIndex] = field.SumValue;
        lookup[index + 2 * maxLookupIndex] = field.WaitShiftValue;
      }

      var path = Path.Combine(_workingDirectory, "SuitScoringLookup.dat");
      using var fileStream = File.Create(path);
      using var writer = new BinaryWriter(fileStream);
      for (var i = 0; i < lookup.Length; i++)
      {
        writer.Write(lookup[i]);
      }
    }

    private readonly string _workingDirectory;

    private static IEnumerable<ConcealedArrangement> CreateAnalyzedWords()
    {
      for (var groupCount = 0; groupCount < 5; groupCount++)
      {
        foreach (var word in EnumerateArrangements(new int[9], new Stack<Block>(), groupCount))
        {
          yield return word;
        }

        for (var i = 0; i < 9; i++)
        {
          var tiles = new int[9];
          tiles[i] = 2;
          var blocks = new Stack<Block>();
          blocks.Push(Block.Pair(i));
          foreach (var word in EnumerateArrangements(tiles, blocks, groupCount))
          {
            yield return word;
          }
        }
      }
    }

    private static IEnumerable<ConcealedArrangement> EnumerateArrangements(int[] tileCounts, Stack<Block> blocks, int remainingBlocks)
    {
      if (remainingBlocks == 0)
      {
        yield return new ConcealedArrangement(tileCounts, blocks);
        yield break;
      }

      for (var i = 0; i < 7; i++)
      {
        // avoid redundancies by looking at the blocks already in the stack
        if (tileCounts[i] < 4 && tileCounts[i + 1] < 4 && tileCounts[i + 2] < 4 && blocks.All(b => b.IsPair || b.IsShuntsu && b.Index <= i))
        {
          tileCounts[i] += 1;
          tileCounts[i + 1] += 1;
          tileCounts[i + 2] += 1;
          blocks.Push(Block.Shuntsu(i));

          foreach (var word in EnumerateArrangements(tileCounts, blocks, remainingBlocks - 1))
          {
            yield return word;
          }

          blocks.Pop();
          tileCounts[i] -= 1;
          tileCounts[i + 1] -= 1;
          tileCounts[i + 2] -= 1;
        }
      }

      for (var i = 0; i < 9; i++)
      {
        // avoid redundancies by looking at the blocks already in the stack
        if (tileCounts[i] < 2 && blocks.All(b => b.IsPair || b.IsShuntsu || b.IsKoutsu && b.Index <= i))
        {
          tileCounts[i] += 3;
          blocks.Push(Block.Koutsu(i));

          foreach (var word in EnumerateArrangements(tileCounts, blocks, remainingBlocks - 1))
          {
            yield return word;
          }

          blocks.Pop();
          tileCounts[i] -= 3;
        }
      }
    }
  }
}
