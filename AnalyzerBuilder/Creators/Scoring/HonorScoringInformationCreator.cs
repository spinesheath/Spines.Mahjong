using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class HonorScoringInformationCreator
  {
    public HonorScoringInformationCreator(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    public void CreateLookup()
    {
      const int maxLookupIndex = 78125;
      var lookup = new long[maxLookupIndex * 2];

      var language = CreateAnalyzedWords();
      foreach (var word in language)
      {
        var index = word.Base5Hash;
        var field = new HonorScoringBitField(word);
        Debug.Assert(lookup[index] == 0 || lookup[index] == field.AndValue);

        lookup[index] = field.AndValue;
        lookup[index + maxLookupIndex] = field.SumValue;
      }

      var path = Path.Combine(_workingDirectory, "HonorScoringLookup.dat");
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
        foreach (var word in EnumerateArrangements(new int[7], new Stack<Block>(), groupCount))
        {
          yield return word;
        }

        for (var i = 0; i < 7; i++)
        {
          var tiles = new int[7];
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
        if (tileCounts[i] < 2 && blocks.All(b => b.IsPair || b.IsKoutsu && b.Index <= i))
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