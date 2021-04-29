using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace AnalyzerBuilder.Classification
{
  internal class StateComparer : IEqualityComparer<State>
  {
    public bool Equals(State? first, State? second)
    {
      if (first == second)
      {
        return true;
      }
      if (first == null || second == null)
      {
        return false;
      }
      return first.TargetStates.SequenceEqual(second.TargetStates);
    }

    public int GetHashCode(State state)
    {
      Debug.Assert(state != null);
      var hc = 1;
      for (var i = 0; i < state.AlphabetSize; ++i)
      {
        hc = unchecked(hc * 17 + (state.TargetStates[i] == null ? 0 : state.TargetStates[i].GetHashCode()));
      }
      return hc;
    }
  }
}