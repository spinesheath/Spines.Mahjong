using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class HonorMeldScoringInformationCreator
  {
    public HonorMeldScoringInformationCreator(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    public void CreateLookup()
    {
      const int maxLookupIndex = 1500625;
      var sumLookup = new long[maxLookupIndex];

      var language = CreateAnalyzedWords();
      foreach (var word in language)
      {
        var field = new HonorMeldScoringBitField(word.Blocks);
        foreach (var index in word.LookupIndexes)
        {
          Debug.Assert(sumLookup[index] == 0 || sumLookup[index] == field.SumValue);

          sumLookup[index] = field.SumValue;
        }
      }

      var path = Path.Combine(_workingDirectory, "HonorMeldSumLookup.dat");
      using var fileStream = File.Create(path);
      using var writer = new BinaryWriter(fileStream);
      for (var i = 0; i < sumLookup.Length; i++)
      {
        writer.Write(sumLookup[i]);
      }
    }

    private readonly string _workingDirectory;

    private static IEnumerable<MeldArrangement> CreateAnalyzedWords()
    {
      for (var groupCount = 0; groupCount < 5; groupCount++)
      {
        foreach (var word in EnumerateArrangements(new int[9], new Stack<Block>(), groupCount))
        {
          yield return word;
        }
      }
    }

    private static IEnumerable<MeldArrangement> EnumerateArrangements(int[] tileCounts, Stack<Block> blocks, int remainingBlocks)
    {
      if (remainingBlocks == 0)
      {
        yield return new MeldArrangement(blocks.ToList());
        yield break;
      }

      for (var i = 0; i < 9; i++)
      {
        // avoid redundancies by looking at the blocks already in the stack
        if (tileCounts[i] < 2 && blocks.All(b => b.IsKoutsu && b.Index <= i))
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

      for (var i = 0; i < 9; i++)
      {
        // avoid redundancies by looking at the blocks already in the stack
        if (tileCounts[i] == 0 && blocks.All(b => b.IsKoutsu || b.IsAnkan && b.Index <= i))
        {
          tileCounts[i] += 4;
          blocks.Push(Block.Ankan(i));

          foreach (var word in EnumerateArrangements(tileCounts, blocks, remainingBlocks - 1))
          {
            yield return word;
          }

          blocks.Pop();
          tileCounts[i] -= 4;
        }
      }

      for (var i = 0; i < 9; i++)
      {
        // avoid redundancies by looking at the blocks already in the stack
        if (tileCounts[i] == 0 && blocks.All(b => b.IsKoutsu || b.IsKantsu || b.IsAnkan || b.IsMinkan && b.Index <= i))
        {
          tileCounts[i] += 4;
          blocks.Push(Block.Minkan(i));

          foreach (var word in EnumerateArrangements(tileCounts, blocks, remainingBlocks - 1))
          {
            yield return word;
          }

          blocks.Pop();
          tileCounts[i] -= 4;
        }
      }
    }
  }
}