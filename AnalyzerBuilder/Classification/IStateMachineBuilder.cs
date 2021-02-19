using System.Collections.Generic;

namespace AnalyzerBuilder.Classification
{
  /// <summary>
  /// An interface for classes that contain information about a state machine.
  /// </summary>
  internal interface IStateMachineBuilder
  {
    /// <summary>
    /// The size of the alphabet.
    /// </summary>
    int AlphabetSize { get; }

    /// <summary>
    /// The transitions for the specified language.
    /// </summary>
    IReadOnlyList<int> Transitions { get; }

    /// <summary>
    /// The states at which the transitions can be entered.
    /// </summary>
    /// <returns>The ids of the states.</returns>
    IReadOnlyList<int> EntryStates { get; }

    /// <summary>
    /// Is the transition one that describes can not be reached with a legal word?
    /// </summary>
    /// <param name="transition">The Id of the transtion.</param>
    /// <returns>True, if the transition can not be reached, false otherwise.</returns>
    bool IsNull(int transition);

    /// <summary>
    /// Is the transition one that describes a result?
    /// </summary>
    /// <param name="transition">The Id of the transtion.</param>
    /// <returns>True, if the transition is a result, false otherwise.</returns>
    bool IsResult(int transition);
  }
}