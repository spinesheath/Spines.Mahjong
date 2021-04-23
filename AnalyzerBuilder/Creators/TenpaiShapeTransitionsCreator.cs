using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AnalyzerBuilder.Classification;

namespace AnalyzerBuilder.Creators
{
  /// <summary>
  /// TileType counts to bitfield with information relevant for yaku check
  /// Bits, from least significant to highest:
  /// 3 ittsuu (123, 456, 789)
  /// 1 junchan
  /// 1 iipeikou
  /// 1 pinfu if wait not in this suit
  /// 9 pinfu if wait in this suit, indexed by wait
  /// 7 shuntsu presence for sanshoku doujun
  /// 9 koutsu presence for sanshoku doukou
  /// 3 ankou count if wait not in this suit
  /// 3*9 ankou count if wait in this suit, indexed by wait
  /// </summary>
  /// <remarks>
  /// meldIds used here:
  ///  0: 123
  ///  1: 234
  /// ...
  ///  6: 789
  ///  7: 111
  /// ...
  /// 15: 999
  /// 16: 11
  /// ...
  /// 24: 99
  /// </remarks>
  internal class TenpaiShapeTransitionsCreator
  {
    public TenpaiShapeTransitionsCreator(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    public IEnumerable<int> Create()
    {
      var transitionsPath = Path.Combine(_workingDirectory, "TenpaiShapeTransitions.txt");
      var valuesPath = Path.Combine(_workingDirectory, "TenpaiShapeValues.txt");
      if (File.Exists(transitionsPath))
      {
        return File.ReadAllLines(transitionsPath).Select(line => Convert.ToInt32(line, CultureInfo.InvariantCulture));
      }
      
      var language = CreateLanguage().ToList();

      var builder = new ClassifierBuilder();
      builder.SetLanguage(language, 5, 9);

      File.WriteAllLines(transitionsPath, builder.Transitions.Select(t => t.ToString(CultureInfo.InvariantCulture)));
      File.WriteAllLines(valuesPath, _indexedValues.OrderBy(p => p.Value).Select(p => p.Key.ToString(CultureInfo.InvariantCulture)));

      return builder.Transitions;
    }

    private readonly string _workingDirectory;
    private readonly Dictionary<long, int> _indexedValues = new Dictionary<long, int>();

    private IEnumerable<WordWithValue> CreateLanguage()
    {
      var singleValueWords = CreateAnalyzedWords();

      var grouped = new Dictionary<int, List<KeyValuePair<List<int>, List<int>>>>();
      foreach (var w in singleValueWords)
      {
        var h = 0;
        foreach (var c in w.Key)
        {
          h *= 5;
          h += c;
        }

        if (grouped.TryGetValue(h, out var list))
        {
          list.Add(w);
        }
        else
        {
          grouped[h] = new List<KeyValuePair<List<int>, List<int>>> {w};
        }
      }

      foreach (var group in grouped.Values)
      {
        var value = 0L;
        
        // ittsuu
        var bestIttsuuArrangement = group.GroupBy(g => g.Value.Intersect(new[] {0, 3, 6}).Count()).OrderByDescending(g => g.Key).First().First().Value;
        foreach (var meldId in bestIttsuuArrangement.Intersect(new[] { 0, 3, 6 }))
        {
          value |= 1L << (0 + meldId / 3);
        }

        // junchan
        if (group.Any(g => g.Value.All(v => v == 0 || v == 6 || v == 7 || v == 15 || v == 16 || v == 24)))
        {
          value |= 1L << 3;
        }
        
        // iipeikou
        if (group.Any(g => g.Value.GroupBy(v => v).Any(v => v.Key < 7 && v.Count() >= 2)))
        {
          value |= 1L << 4;
        }

        // non wait pinfu
        if (group.Any(g => g.Value.All(v => v < 7 || v > 15)))
        {
          value |= 1L << 5;
        }

        // pinfu with wait
        for (var i = 0; i < 9; i++)
        {
          if (group.Any(g => g.Value.All(v => v < 7 || v > 15) && g.Value.Any(v => v == i || v + 2 == i)))
          {
            value |= 1L << (6 + i);
          }
        }

        // sanshoku (impossible if there are 3 groups in the same suit)
        if (group.All(g => g.Value.Count(v => v < 16) < 3))
        {
          // doujun
          for (var i = 0; i < 7; i++)
          {
            if (group.Any(g => g.Value.Contains(i)))
            {
              value |= 1L << (15 + i);
            }
          }

          // doukou
          for (var i = 0; i < 9; i++)
          {
            if (group.Any(g => g.Value.Any(v => v == i + 7)))
            {
              value |= 1L << (22 + i);
            }
          }
        }

        // ankou count without wait, 3 bit
        value |= (long) group.Max(g => g.Value.Count(v => v >= 7 && v <= 15)) << 31;

        // ankou count with wait, 3 bit per tile type
        for (var i = 0; i < 9; i++)
        {
          // koutsu that are not affected by wait, plus koutsu on the wait (0 or 1) times shuntsu on the wait (0 or 1).
          value |= (long)group.Max(g => g.Value.Count(v => v >= 7 && v <= 15 && v != i + 7) + g.Value.Count(v => v == i + 7) * g.Value.Count(v => v <= i && v + 2 >= i && v < 7)) << (34 + 3 * i);
        }

        if (!_indexedValues.TryGetValue(value, out var valueIndex))
        {
          valueIndex = _indexedValues.Count;
          _indexedValues[value] = valueIndex;
        }

        yield return new WordWithValue(group[0].Key, valueIndex);
      }
    }

    private static IEnumerable<KeyValuePair<List<int>, List<int>>> CreateAnalyzedWords()
    {
      for (var groupCount = 0; groupCount < 5; groupCount++)
      {
        foreach (var word in EnumerateArrangements(new int[9], new Stack<int>(), groupCount))
        {
          yield return word;
        }

        for (var i = 0; i < 9; i++)
        {
          var tiles = new int[9];
          tiles[i] = 2;
          var meldIds = new Stack<int>();
          meldIds.Push(7 + 9 + i);
          foreach (var word in EnumerateArrangements(tiles, meldIds, groupCount))
          {
            yield return word;
          }
        }
      }
    }

    private static IEnumerable<KeyValuePair<List<int>, List<int>>> EnumerateArrangements(int[] tiles, Stack<int> meldIds, int remainingGroups)
    {
      if (remainingGroups == 0)
      {
        yield return new KeyValuePair<List<int>, List<int>>(tiles.ToList(), meldIds.ToList());
        yield break;
      }

      for (var i = 0; i < 7; i++)
      {
        // remove redundancies by looking at the meldIds already in the stack
        if (tiles[i] < 4 && tiles[i + 1] < 4 && tiles[i + 2] < 4 && meldIds.All(m => m <= i || m > 15))
        {
          tiles[i] += 1;
          tiles[i + 1] += 1;
          tiles[i + 2] += 1;
          meldIds.Push(i);
          
          foreach (var word in EnumerateArrangements(tiles, meldIds, remainingGroups - 1))
          {
            yield return word;
          }

          meldIds.Pop();
          tiles[i] -= 1;
          tiles[i + 1] -= 1;
          tiles[i + 2] -= 1;
        }
      }

      for (var i = 0; i < 9; i++)
      {
        var meldId = i + 7;
        // remove redundancies by looking at the meldIds already in the stack
        if (tiles[i] < 2 && meldIds.All(m => m <= meldId || m > 15))
        {
          tiles[i] += 3;
          meldIds.Push(meldId);
          
          foreach (var word in EnumerateArrangements(tiles, meldIds, remainingGroups - 1))
          {
            yield return word;
          }

          meldIds.Pop();
          tiles[i] -= 3;
        }
      }
    }
  }
}