using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using AnalyzerBuilder.Classification;

namespace AnalyzerBuilder.Creators
{
  internal class SuitFirstPhaseBuilder : IStateMachineBuilder
  {
    public SuitFirstPhaseBuilder(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    /// <summary>
    /// The size of the alphabet.
    /// </summary>
    public int AlphabetSize { get; } = 26;

    /// <summary>
    /// The transitions for the specified language.
    /// </summary>
    public IReadOnlyList<int> Transitions { get; private set; }

    /// <summary>
    /// The states at which the transitions can be entered.
    /// </summary>
    /// <returns>The ids of the states.</returns>
    public IReadOnlyList<int> EntryStates => new[] {0};

    public void SetLanguage()
    {
      var newLanguage = CreateNewLanguage().ToList();

      var columns = new List<int[]>();
      // Value to stateId.
      var finalValueToStateId = new Dictionary<int, int>();
      columns.Add(CreateArray(26));
      foreach (var word in newLanguage)
      {
        var current = 0;
        foreach (var c in word)
        {
          if (columns[current][c] == -1)
          {
            columns.Add(CreateArray(26));
            columns[current][c] = columns.Count - 1;
          }
          current = columns[current][c];
        }

        if (!finalValueToStateId.ContainsKey(word.Value))
        {
          var count = finalValueToStateId.Count;
          finalValueToStateId.Add(word.Value, count);
        }
        columns[current][25] = finalValueToStateId[word.Value];
      }

      var incoming = GetIncomingTransitionTable(columns, finalValueToStateId);

      // Apply Hopcroft
      var normalStates = Enumerable.Range(0, columns.Count);
      var finalStates = Enumerable.Range(columns.Count, finalValueToStateId.Count);
      var hopcroft = new Hopcroft(normalStates, finalStates.Select(fs => fs.Yield()), 26, (a, c) => a.SelectMany(aa => incoming[aa][c]));
      var equivalenceGroups = hopcroft.EquivalenceGroups;
      var oldToNewIds =
        equivalenceGroups.OrderBy(g => g.Min())
          .SelectMany((g, i) => g.Where(s => s < columns.Count).Select(s => new KeyValuePair<int, int>(s, i)))
          .ToDictionary(k => k.Key, k => k.Value);

      var nonFinalStateCount = equivalenceGroups.Count(g => g.Any(e => e < columns.Count));
      var newTransitions = CreateArray(nonFinalStateCount * 26);
      for (var i = 0; i < columns.Count; ++i)
      {
        var column = columns[i];
        var newId = oldToNewIds[i];
        for (var c = 0; c < 25; ++c)
        {
          var oldTransition = column[c];
          if (oldTransition == -1)
          {
            continue; // Don't do anything for null transitions.
          }
          var newTransiton = oldToNewIds[oldTransition] * 26;
          newTransitions[newId * 26 + c + 1] = newTransiton; // Shift the transitions one character to the back.
        }
      }

      // Insert final values
      var meldCountsToValue = new Dictionary<int, HashSet<int>>();
      for (var i = 0; i < 5; ++i)
      {
        meldCountsToValue.Add(i, new HashSet<int>());
      }
      foreach (var word in newLanguage)
      {
        meldCountsToValue[word.Count].Add(word.Value);
      }
      var meldCountsToOldToNewValue = new List<Dictionary<int, int>>();
      for (var i = 0; i < 5; ++i)
      {
        var offsetPath = Path.Combine(_workingDirectory, $"o_SuitSecondPhase{i}.txt");
        var offsets =
          File.ReadAllLines(offsetPath).Select(line => Convert.ToInt32(line, CultureInfo.InvariantCulture)).ToList();

        // Entry states are ordered the same way in phase two.
        var orderedEntryStates = meldCountsToValue[i].OrderBy(x => x);
        var d = new Dictionary<int, int>();
        foreach (var entryState in orderedEntryStates)
        {
          var stateId = d.Count;
          d.Add(entryState, stateId * 5 - offsets[stateId]);
        }
        meldCountsToOldToNewValue.Add(d);
      }
      foreach (var word in newLanguage)
      {
        var current = 0;
        foreach (var c in word)
        {
          current = newTransitions[current + c + 1];
        }
        newTransitions[current] = meldCountsToOldToNewValue[word.Count][word.Value];
      }

      Transitions = newTransitions;
    }

    private static int[] CreateArray(int length)
    {
      var item = new int[length];
      Array.Fill(item, -1);
      return item;
    }

    /// <summary>
    /// Is the transition one that describes can not be reached with a legal word?
    /// </summary>
    /// <param name="transition">The Id of the transtion.</param>
    /// <returns>True, if the transition can not be reached, false otherwise.</returns>
    public bool IsNull(int transition)
    {
      return Transitions[transition] == -1;
    }

    /// <summary>
    /// Is the transition one that describes a result?
    /// </summary>
    /// <param name="transition">The Id of the transtion.</param>
    /// <returns>True, if the transition is a result, false otherwise.</returns>
    public bool IsResult(int transition)
    {
      return transition % AlphabetSize == 0;
    }

    private readonly string _workingDirectory;

    private IEnumerable<WordWithValue> CreateNewLanguage()
    {
      var meldWords = GetMeldWords();

      // 3 bit per tile count, 30 bit total
      // create dictionary from 30 bit to result
      var results = new Dictionary<int, int>();
      foreach (var word in meldWords)
      {
        var id = word.Word.Select((c, i) => c << i * 3).Sum();
        results.Add(id, word.Value);
      }

      // create 25x25x25x25 language
      var newBaseLanguage = GetMeldLanguage();

      // count the used tiles
      // get result from dictionary
      foreach (var w in newBaseLanguage)
      {
        var word = w.ToList();
        var oldWord = new int[10];
        foreach (var c in word)
        {
          oldWord[0] += 1;
          if (c < 7)
          {
            oldWord[c + 1] += 1;
            oldWord[c + 2] += 1;
            oldWord[c + 3] += 1;
          }
          else if (c < 16)
          {
            oldWord[c - 6] += 3;
          }
          else if (c < 25)
          {
            oldWord[c - 15] += 4;
          }
        }
        if (oldWord.Any(c => c > 4))
        {
          continue;
        }
        var id = oldWord.Select((c, i) => c << i * 3).Sum();
        if (results.ContainsKey(id))
        {
          yield return new WordWithValue(word, results[id]);
        }
      }
    }

    private IEnumerable<WordWithValue> GetMeldWords()
    {
      var transitions = new UnweightedSuitTransitionsCreator(_workingDirectory).Create().ToList();

      var meldWords = new List<WordWithValue>();
      var baseLanguage = Enumerable.Repeat(Enumerable.Range(0, 5), 10).CartesianProduct();
      foreach (var word in baseLanguage)
      {
        var w = word.ToList();
        var current = 0;
        foreach (var c in w)
        {
          current = transitions[current + c];
          if (current == -1)
          {
            break;
          }
        }
        if (current != -1)
        {
          meldWords.Add(new WordWithValue(w, current));
        }
      }
      return meldWords;
    }

    private static List<List<List<int>>> GetIncomingTransitionTable(IReadOnlyList<int[]> columns,
      IReadOnlyDictionary<int, int> finalValues)
    {
      var incoming = new List<List<List<int>>>();
      for (var i = 0; i < columns.Count; ++i)
      {
        incoming.Add(new List<List<int>>());
        for (var c = 0; c < 26; ++c)
        {
          incoming[i].Add(new List<int>());
        }
      }
      for (var i = 0; i < columns.Count; ++i)
      {
        for (var c = 0; c < 25; ++c)
        {
          var t = columns[i][c];
          if (t != -1)
          {
            incoming[t][c].Add(i);
          }
        }
      }
      for (var i = 0; i < finalValues.Count; ++i)
      {
        incoming.Add(new List<List<int>>());
        for (var c = 0; c < 26; ++c)
        {
          incoming[i + columns.Count].Add(new List<int>());
        }
        for (var j = 0; j < columns.Count; ++j)
        {
          if (columns[j][25] == i)
          {
            incoming[i + columns.Count][25].Add(j);
          }
        }
      }
      return incoming;
    }

    /// <summary>
    /// 0 to 4 characters from 0 to 24.
    /// </summary>
    private static IEnumerable<IEnumerable<int>> GetMeldLanguage()
    {
      return
        Enumerable.Range(0, 5)
          .SelectMany(length => Enumerable.Repeat(Enumerable.Range(0, 25), length).CartesianProduct());
    }
  }
}