using System;
using System.Collections.Generic;

namespace AnalyzerBuilder.Classification
{
  internal class StateManager
  {
    public StateManager(int alphabetSize, int wordLength)
    {
      _alphabetSize = alphabetSize;
      _heights = wordLength + 1;
      _uniqueStates = new Dictionary<State, State>[_heights];
      for (var i = 0; i < _heights; ++i)
      {
        _uniqueStates[i] = new Dictionary<State, State>(new StateComparer());
      }
      _finalStates = new Dictionary<int, FinalState>();
      StartingState = new State(_alphabetSize);
      InsertUniqueState(StartingState, _heights - 1);
    }

    public State StartingState { get; }

    /// <summary>
    /// Indices of the transitions that contain a final value in the transitions array.
    /// </summary>
    public ISet<int> ResultIndexes { get; private set; }

    /// <summary>
    /// If this method is called before all states are finalized, the result will not be correct.
    /// Usage:
    /// int current = 0;
    /// foreach(int c in word)
    /// current = table[current + c];
    /// return current;
    /// </summary>
    public IReadOnlyList<int> Transitions => _transitions;

    /// <summary>
    /// Assigns each state a unique Id and creates a transition table.
    /// </summary>
    public void CompactTransitions()
    {
      // Give each state a unique Id.
      var id = 0;
      for (var i = _heights - 1; i >= 0; --i)
      {
        var row = _uniqueStates[i];
        foreach (var state in row)
        {
          state.Value.Id = id;
          id += 1;
        }
      }
      // Create the actual machine.
      _transitions = new int[id * _alphabetSize];
      Array.Fill(_transitions, -1);
      ResultIndexes = new HashSet<int>();
      for (var i = _heights - 1; i > 0; --i)
      {
        var row = _uniqueStates[i];
        foreach (var svp in row)
        {
          var state = svp.Value;
          for (var j = 0; j < _alphabetSize; ++j)
          {
            var nextState = state.Advance(j);
            if (nextState != null)
            {
              var index = GetIndex(state.Id, j);
              if (i == 1)
              {
                var finalState = (FinalState) nextState;
                _transitions[index] = finalState.Value;
                ResultIndexes.Add(index);
              }
              else
              {
                _transitions[index] = nextState.Id * _alphabetSize;
              }
            }
          }
        }
      }
    }

    public void RemoveUniqueState(State state, int height)
    {
      if (!_uniqueStates[height].Remove(state))
      {
        throw new InvalidOperationException("State isn't unique.");
      }
    }

    public void InsertUniqueState(State state, int height)
    {
      _uniqueStates[height].Add(state, state);
    }

    public State TryGetEquivalentUniqueState(State state, int height)
    {
      var statesAtHeight = _uniqueStates[height];
      var isRedundant = statesAtHeight.TryGetValue(state, out var uniqueState);
      return isRedundant ? uniqueState : null;
    }

    public State GetOrCreateFinalState(int value)
    {
      if (_finalStates.ContainsKey(value))
      {
        return _finalStates[value];
      }
      var final = new FinalState(_alphabetSize, value);
      _finalStates.Add(value, final);
      return final;
    }

    private readonly int _alphabetSize;
    private readonly Dictionary<int, FinalState> _finalStates;
    private readonly int _heights;
    private readonly Dictionary<State, State>[] _uniqueStates;
    private int[] _transitions;

    /// <summary>
    /// The index of a transition in the transition table.
    /// </summary>
    private int GetIndex(int state, int character)
    {
      return _alphabetSize * state + character;
    }
  }
}