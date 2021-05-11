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
      const int maxLookupIndex = 456976;
      var lookup = new long[maxLookupIndex * 3];

      var language = CreateAnalyzedWords();
      foreach (var word in language)
      {
        var field = new HonorMeldScoringBitField(word.Blocks);
        foreach (var index in word.LookupIndexes)
        {
          Debug.Assert(lookup[index] == 0 || lookup[index] == field.AndValue);
          Debug.Assert(lookup[index + maxLookupIndex] == 0 || lookup[index + maxLookupIndex] == field.OrValue);

          lookup[index] = field.AndValue;
          lookup[index + maxLookupIndex] = field.OrValue;
          lookup[index + 2 * maxLookupIndex] = field.SumValue;
        }
      }

      var path = Path.Combine(_workingDirectory, "HonorMeldScoringLookup.dat");
      using var fileStream = File.Create(path);
      using var writer = new BinaryWriter(fileStream);
      for (var i = 0; i < lookup.Length; i++)
      {
        writer.Write(lookup[i]);
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

      for (var i = 0; i < 9; i++)
      {
        // avoid redundancies by looking at the blocks already in the stack
        if (tileCounts[i] == 0 && blocks.All(b => b.IsPair || b.IsShuntsu || b.IsKoutsu || b.IsKantsu && b.Index <= i))
        {
          tileCounts[i] += 4;
          blocks.Push(Block.Kantsu(i));

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