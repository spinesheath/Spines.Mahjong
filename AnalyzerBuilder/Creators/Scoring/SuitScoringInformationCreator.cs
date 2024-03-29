﻿using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class SuitScoringInformationCreator
  {
    public SuitScoringInformationCreator(string directory)
    {
      _directory = directory;
    }

    public void CreateLookup(FootprintCollection footprints)
    {
      const int maxLookupIndex = 1953125; // 5^9
      var orLookup = new long[maxLookupIndex];
      var waitShiftLookup = new long[maxLookupIndex];

      var language = CreateAnalyzedWords();
      var groupedByHash = language.GroupBy(w => w.Base5Hash);
      var arrangementGroups = groupedByHash.Select(g => new ArrangementGroup(g)).ToList();

      foreach (var arrangementGroup in arrangementGroups)
      {
        var footprintCreator = new FuFootprintCreator(arrangementGroup);

        var footprintIndex = footprints.IndexOf(footprintCreator.Footprint);
        
        var base5Hash = arrangementGroup.Base5Hash;
        var field = new SuitScoringBitField(arrangementGroup, footprintIndex);

        Debug.Assert(orLookup[base5Hash] == 0 || orLookup[base5Hash] == field.OrValue);
        orLookup[base5Hash] = field.OrValue;

        Debug.Assert(waitShiftLookup[base5Hash] == 0 || waitShiftLookup[base5Hash] == field.WaitShiftValue);
        waitShiftLookup[base5Hash] = field.WaitShiftValue;
      }

      Write("SuitOrLookup.dat", orLookup);
      Write("SuitWaitShiftLookup.dat", waitShiftLookup);
    }

    private void Write(string filename, long[] data)
    {
      var path = Path.Combine(_directory, filename);
      using var fileStream = File.Create(path);
      using var writer = new BinaryWriter(fileStream);
      for (var i = 0; i < data.Length; i++)
      {
        writer.Write(data[i]);
      }
    }

    private readonly string _directory;

    private static IEnumerable<Arrangement> CreateAnalyzedWords()
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

      foreach (var word in EnumerateChiitoiArrangements())
      {
        yield return word;
      }

      foreach (var word in EnumerateKokushiArrangements())
      {
        yield return word;
      }
    }

    private static IEnumerable<Arrangement> EnumerateChiitoiArrangements()
    {
      for (var i = 0; i < (1 << 9); i++)
      {
        var tiles = new int[9];
        var blocks = new List<Block>();

        var j = 1;
        for (var k = 0; k < 9; k++)
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
          yield return new Arrangement(tiles, blocks);
        }
        else if (blocks.Count <= 7)
        {
          yield return new Arrangement(tiles, Enumerable.Empty<Block>());
        }
      }
    }

    private static IEnumerable<Arrangement> EnumerateKokushiArrangements()
    {
      var tiles = new int[9];
      tiles[0] = 1;
      tiles[8] = 1;
      yield return new Arrangement(tiles, Enumerable.Empty<Block>());
      tiles[0] = 2;
      yield return new Arrangement(tiles, Enumerable.Empty<Block>());
      tiles[0] = 1;
      tiles[8] = 2;
      yield return new Arrangement(tiles, Enumerable.Empty<Block>());
    }

    private static IEnumerable<Arrangement> EnumerateArrangements(int[] tileCounts, Stack<Block> blocks, int remainingBlocks)
    {
      if (remainingBlocks == 0)
      {
        yield return new Arrangement(tileCounts, blocks);
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
