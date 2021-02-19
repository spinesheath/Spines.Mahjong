using System;
using System.Collections.Generic;
using System.Linq;
using AnalyzerBuilder.Classification;

namespace AnalyzerBuilder.Creators
{
  internal class ProgressiveKokushiBuilder : IStateMachineBuilder
  {
    public ProgressiveKokushiBuilder()
    {
      var baseLanguage = new[] {Enumerable.Range(0, 14), Enumerable.Range(0, 8)}.CartesianProduct().Select(tt => tt.ToList());
      var normalStates = baseLanguage.Where(tt => tt[0] + tt[1] * 2 <= 14).Select(tt => GetState(tt[0], tt[1])).OrderBy(x => x).ToList();
      var finalStates = normalStates.GroupBy(GetValue);

      var hopcroft = new Hopcroft(normalStates, finalStates, AlphabetSize - 1, GetIncomingStates);
      var equivalenceGroups = hopcroft.EquivalenceGroups;

      var oldToNewIds = equivalenceGroups.OrderBy(g => g.Min()).SelectMany((g, i) => g.Select(s => new KeyValuePair<int, int>(s, i))).ToDictionary(k => k.Key, k => k.Value);

      var transitions = new int[equivalenceGroups.Count * AlphabetSize];
      Array.Fill(transitions, -1);

      for (var i = 0; i < normalStates.Count; ++i)
      {
        var oldState = normalStates[i];
        var newId = oldToNewIds[oldState];
        for (var c = 0; c < AlphabetSize - 1; ++c)
        {
          var oldTransition = GetNext(oldState, c);
          if (oldTransition == -1)
          {
            continue; // Don't do anything for null transitions.
          }
          var newTransition = oldToNewIds[oldTransition] * AlphabetSize;
          transitions[newId * AlphabetSize + c + 1] = newTransition; // Shift the transitions one character to the back.
        }
        transitions[newId * AlphabetSize] = GetValue(oldState); // Place value at the front.
      }

      Transitions = transitions;
    }

    /// <summary>
    /// The transitions for the specified language.
    /// </summary>
    public IReadOnlyList<int> Transitions { get; }

    /// <summary>
    /// 4 regular characters and 1 for the final value.
    /// </summary>
    public int AlphabetSize => 5;

    /// <summary>
    /// The states at which the transitions can be entered.
    /// </summary>
    /// <returns>The ids of the states.</returns>
    public IReadOnlyList<int> EntryStates => new int[0];

    /// <summary>
    /// Is the transition one that describes can not be reached with a legal word?
    /// </summary>
    /// <param name="transition">The Id of the transition.</param>
    /// <returns>True, if the transition can not be reached, false otherwise.</returns>
    public bool IsNull(int transition)
    {
      return Transitions[transition] == -1;
    }

    /// <summary>
    /// Is the transition one that describes a result?
    /// </summary>
    /// <param name="transition">The Id of the transition.</param>
    /// <returns>True, if the transition is a result, false otherwise.</returns>
    public bool IsResult(int transition)
    {
      return transition % AlphabetSize == 0;
    }

    private static int GetValue(int state)
    {
      var singles = state % 16;
      var pairs = state / 16;
      return 14 - (pairs > 0 ? 1 : 0) - pairs - singles; // Shanten + 1 so value is never negative.
    }

    private static int GetNext(int state, int c)
    {
      var singles = state % 16;
      var pairs = state / 16;
      switch (c)
      {
        case 0:
          return GetState(singles + 1, pairs);
        case 1:
          return GetState(singles - 1, pairs + 1);
        case 2:
          return GetState(singles + 1, pairs - 1);
        case 3:
          return GetState(singles - 1, pairs);
      }
      return -1;
    }

    private static IEnumerable<int> GetIncomingStates(HashSet<int> a, int c)
    {
      foreach (var state in a)
      {
        var singles = state % 16;
        var pairs = state / 16;
        var previous = -1;
        switch (c)
        {
          case 0:
            previous = GetState(singles - 1, pairs);
            break;
          case 1:
            previous = GetState(singles + 1, pairs - 1);
            break;
          case 2:
            previous = GetState(singles - 1, pairs + 1);
            break;
          case 3:
            previous = GetState(singles + 1, pairs);
            break;
        }
        if (previous != -1)
        {
          yield return previous;
        }
      }
    }

    private static int GetState(int singles, int pairs)
    {
      if (singles > 13 || pairs > 7 || singles < 0 || pairs < 0)
      {
        return -1;
      }
      if (singles + 2 * pairs > 14)
      {
        return -1;
      }
      return singles + 16 * pairs;
    }
  }
}