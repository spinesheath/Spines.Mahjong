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
      const int maxLookupIndex = 78125; // 5^7
      var sumLookup = new long[maxLookupIndex];
      var waitShiftLookup = new long[maxLookupIndex];

      var language = CreateAnalyzedWords();
      foreach (var word in language)
      {
        var index = word.Base5Hash;
        var field = new HonorScoringBitField(word);

        Debug.Assert(sumLookup[index] == 0 || sumLookup[index] == field.SumValue);
        sumLookup[index] = field.SumValue;

        Debug.Assert(waitShiftLookup[index] == 0 || waitShiftLookup[index] == field.WaitShiftValue);
        waitShiftLookup[index] = field.WaitShiftValue;
      }

      Write("HonorSumLookup.dat", sumLookup);
      Write("HonorWaitShiftLookup.dat", waitShiftLookup);
    }

    private void Write(string filename, long[] data)
    {
      var path = Path.Combine(_workingDirectory, filename);
      using var fileStream = File.Create(path);
      using var writer = new BinaryWriter(fileStream);
      for (var i = 0; i < data.Length; i++)
      {
        writer.Write(data[i]);
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

      foreach (var word in EnumerateChiitoiArrangements())
      {
        yield return word;
      }

      foreach (var word in EnumerateKokushiArrangements())
      {
        yield return word;
      }
    }

    private static IEnumerable<ConcealedArrangement> EnumerateChiitoiArrangements()
    {
      for (var i = 0; i < (1 << 7); i++)
      {
        var tiles = new int[7];
        var blocks = new List<Block>();

        var j = 1;
        for (var k = 0; k < 7; k++)
        {
          var hasBit = (i & j) != 0;
          if (hasBit)
          {
            tiles[k] = 2;
            blocks.Add(Block.Pair(k));
          }
          j <<= 1;
        }

        if (blocks.Count == 1)
        {
          yield return new ConcealedArrangement(tiles, blocks);
        }
        else
        {
          yield return new ConcealedArrangement(tiles, Enumerable.Empty<Block>());
        }
      }
    }

    private static IEnumerable<ConcealedArrangement> EnumerateKokushiArrangements()
    {
      var tiles = Enumerable.Repeat(1, 7).ToArray();
      yield return new ConcealedArrangement(tiles, Enumerable.Empty<Block>());
      for (var i = 0; i < 7; i++)
      {
        tiles[i] = 2;
        yield return new ConcealedArrangement(tiles, Enumerable.Empty<Block>());
        tiles[i] = 1;
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