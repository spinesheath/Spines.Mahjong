using System;
using System.Collections.Generic;

namespace AnalyzerBuilder.Classification
{
  internal class State
  {
    public State(int alphabetSize)
    {
      _incomingTransitions = 0;
      _targetStates = new State[alphabetSize];
    }

    public int Id { get; set; }

    public int AlphabetSize => TargetStates.Count;

    public IReadOnlyList<State> TargetStates => _targetStates;

    public bool IsConfluenceState => _incomingTransitions > 1;

    public State Clone(int alphabetSize)
    {
      var state = new State(alphabetSize);
      for (var i = 0; i < alphabetSize; ++i)
      {
        if (HasTransition(i))
        {
          state.CreateOutTransition(Advance(i)!, i);
        }
      }
      return state;
    }

    public bool HasTransition(int character)
    {
      return Advance(character) != null;
    }

    public State? Advance(int character)
    {
      return TargetStates[character];
    }

    public void RedirectOutTransition(State target, int character)
    {
      if (HasTransition(character))
      {
        var nextState = Advance(character)!;
        nextState._incomingTransitions -= 1;
        SetTransition(target, character);
      }
      else
      {
        throw new InvalidOperationException("Transition doesn't exist.");
      }
    }

    public void CreateOutTransition(State target, int character)
    {
      if (HasTransition(character))
      {
        throw new InvalidOperationException("Transition already exists.");
      }
      SetTransition(target, character);
    }

    private readonly State[] _targetStates;
    private int _incomingTransitions;

    private void SetTransition(State target, int character)
    {
      _targetStates[character] = target;
      target._incomingTransitions += 1;
    }
  }
}