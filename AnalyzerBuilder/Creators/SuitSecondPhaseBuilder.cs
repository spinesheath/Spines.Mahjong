using System;
using System.Collections.Generic;
using System.Linq;
using AnalyzerBuilder.Classification;

namespace AnalyzerBuilder.Creators
{
  internal class SuitSecondPhaseBuilder : IStateMachineBuilder
  {
    public SuitSecondPhaseBuilder(string workingDirectory, int meldCount)
    {
      _workingDirectory = workingDirectory;
      _meldCount = meldCount;
    }

    /// <summary>
    /// The transitions for the specified language.
    /// </summary>
    public IReadOnlyList<int> Transitions { get; private set; } = new List<int>();

    /// <summary>
    /// The states at which the transitions can be entered.
    /// </summary>
    /// <returns>The ids of the states.</returns>
    public IReadOnlyList<int> EntryStates { get; private set; } = new List<int>();

    /// <summary>
    /// The size of the alphabet.
    /// </summary>
    public int AlphabetSize => 5;

    public void SetLanguage()
    {
      var transitions = new UnweightedSuitTransitionsCreator(_workingDirectory).Create().ToList();

      var entryStates = GetEntryStates(transitions);
      var concealedStates = GetConcealedStates(entryStates, transitions);

      var oldToNewTransitions = new Dictionary<int, int>();
      var orderedEntryStates = entryStates.OrderBy(x => x);
      foreach (var entryState in orderedEntryStates)
      {
        oldToNewTransitions.Add(entryState, oldToNewTransitions.Count);
      }
      foreach (var state in concealedStates.Except(entryStates))
      {
        oldToNewTransitions.Add(state, oldToNewTransitions.Count);
      }
      _statesWithFinalValues.Clear();

      var concealedTransitions = CreateArray(concealedStates.Count * 5);
      foreach (var state in concealedStates)
      {
        foreach (var c in Enumerable.Range(0, 5))
        {
          var next = transitions[state + c];
          if (next == -1)
          {
            continue; // null transitions
          }
          if (oldToNewTransitions.ContainsKey(next))
          {
            concealedTransitions[oldToNewTransitions[state] * 5 + c] = oldToNewTransitions[next] * 5;
            // normal transitions
          }
          else
          {
            _statesWithFinalValues.Add(oldToNewTransitions[state]);
            concealedTransitions[oldToNewTransitions[state] * 5 + c] = next; // final values
          }
        }
      }

      Transitions = concealedTransitions;
      EntryStates = entryStates.Select(e => oldToNewTransitions[e]).ToList();
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
      return _statesWithFinalValues.Contains(transition / 5);
    }

    private readonly string _workingDirectory;
    private readonly int _meldCount;
    private readonly HashSet<int> _statesWithFinalValues = new HashSet<int>();

    private HashSet<int> GetEntryStates(List<int> transitions)
    {
      var entryStates = new HashSet<int>();
      var meldLanguage = _meldCount.Yield().Yield().Concat(Enumerable.Repeat(Enumerable.Range(0, 5), 9)).CartesianProduct();

      foreach (var word in meldLanguage)
      {
        var current = 0;
        foreach (var c in word)
        {
          current = transitions[current + c];
          if (current == -1)
          {
            break;
          }
        }
        if (current != -1)
        {
          entryStates.Add(current);
        }
      }
      return entryStates;
    }

    private static HashSet<int> GetConcealedStates(HashSet<int> entryStates, List<int> transitions)
    {
      var concealedStates = new HashSet<int>();

      var statesInCurrentLevel = new HashSet<int>(entryStates);
      for (var i = 0; i < 9; ++i)
      {
        var statesInPreviousLevel = statesInCurrentLevel;
        statesInCurrentLevel = new HashSet<int>();
        foreach (var state in statesInPreviousLevel)
        {
          concealedStates.Add(state);

          foreach (var c in Enumerable.Range(0, 5))
          {
            var n = transitions[state + c];
            if (n == -1)
            {
              continue;
            }
            statesInCurrentLevel.Add(n);
          }
        }
      }
      return concealedStates;
    }
  }
}