using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using AnalyzerBuilder.Classification;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class SuitMeldScoringInformationCreator
  {
    public SuitMeldScoringInformationCreator(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    public IEnumerable<int> CreateTransitions()
    {
      var transitionsPath = Path.Combine(_workingDirectory, "SuitMeldScoringInformationTransitions.txt");
      var valuesPath = Path.Combine(_workingDirectory, "SuitMeldScoringInformationValues.txt");
      if (File.Exists(transitionsPath))
      {
        return File.ReadAllLines(transitionsPath).Select(line => Convert.ToInt32(line, CultureInfo.InvariantCulture));
      }

      var language = CreateLanguage().ToList();

      var builder = new ClassifierBuilder();
      // TODO alphabet size and word length? Or should this just be a direct lookup instead?
      builder.SetLanguage(language, 5, 9);

      File.WriteAllLines(transitionsPath, builder.Transitions.Select(t => t.ToString(CultureInfo.InvariantCulture)));
      File.WriteAllLines(valuesPath, _valueToValueIndex.OrderBy(p => p.Value).Select(p => p.Key.ToString(CultureInfo.InvariantCulture)));

      return builder.Transitions;
    }

    public void CreateLookup()
    {
      const int maxLookupIndex = 456976;
      var lookup = new long[maxLookupIndex * 2];

      var language = CreateAnalyzedWords();
      foreach (var word in language)
      {
        var field = new SuitMeldScoringBitField(word.Blocks);
        foreach (var index in word.LookupIndexes)
        {
          Debug.Assert(lookup[index] == 0 || lookup[index] == field.AndValue);
          Debug.Assert(lookup[index + maxLookupIndex] == 0 || lookup[index + maxLookupIndex] == field.OrValue);

          lookup[index] = field.AndValue;
          lookup[index + maxLookupIndex] = field.OrValue;
        }
      }
      
      var path = Path.Combine(_workingDirectory, "SuitMeldScoringLookup.dat");
      using var fileStream = File.Create(path);
      using var writer = new BinaryWriter(fileStream);
      for (var i = 0; i < lookup.Length; i++)
      {
        writer.Write(lookup[i]);
      }
    }

    private readonly string _workingDirectory;
    private readonly Dictionary<long, int> _valueToValueIndex = new Dictionary<long, int>();

    private IEnumerable<WordWithValue> CreateLanguage()
    {
      var words = CreateAnalyzedWords();
      
      foreach (var word in words)
      {
        var suitMeld = new SuitMeldScoringBitField(word.Blocks);
        
        if (!_valueToValueIndex.TryGetValue(suitMeld.AndValue, out var valueIndex))
        {
          valueIndex = _valueToValueIndex.Count;
          _valueToValueIndex[suitMeld.AndValue] = valueIndex;
        }

        yield return new WordWithValue(word.Blocks.Select(b => b.Id), valueIndex);
      }
    }

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

      for (var i = 0; i < 7; i++)
      {
        // TODO probably want redundancies here, so order of melds doesn't matter
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
