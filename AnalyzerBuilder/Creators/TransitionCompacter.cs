using System.Collections.Generic;
using System.Linq;
using AnalyzerBuilder.Classification;

namespace AnalyzerBuilder.Creators
{
  internal class TransitionCompacter
  {
    /// <summary>
    /// Compacts transitions by eliminating trailing null tansitions.
    /// I.e if the last couple characters in the alphabet, for one state don't lead to another state, these cells can be
    /// eliminated from the table.
    /// Transitions that point to a later state are adjusted by the amount of eliminated cells.
    /// </summary>
    public TransitionCompacter(IStateMachineBuilder builder)
    {
      // Build a set of indices that can be skipped and a table of offsets for adjusting the remaining transitions.
      var skippedIndices = new HashSet<int>();
      var offsetMap = new int[builder.Transitions.Count];
      var skipTotal = 0;
      for (var i = 0; i < builder.Transitions.Count; i += builder.AlphabetSize)
      {
        // Count the trailing nulls.
        var transitionsToKeep = builder.AlphabetSize;
        for (; transitionsToKeep > 0; transitionsToKeep--)
        {
          var transition = i + transitionsToKeep - 1;
          if (!builder.IsNull(transition) || builder.IsResult(transition))
            // Results and normal transitions can't be skipped.
          {
            break;
          }
          skippedIndices.Add(transition);
        }
        // Build the set and table.
        var toSkip = builder.AlphabetSize - transitionsToKeep;
        skipTotal += toSkip;
        for (var j = 0; j < builder.AlphabetSize; ++j)
        {
          var index = i + builder.AlphabetSize + j;
          if (index >= offsetMap.Length)
          {
            break;
          }
          offsetMap[index] = skipTotal;
        }
      }

      // Adjust the remaining transitions.
      var clone = builder.Transitions.ToList();
      for (var i = 0; i < clone.Count; ++i)
      {
        if (builder.IsNull(i) || builder.IsResult(i)) // nulls and results are not adjusted.
        {
          continue;
        }

        clone[i] -= offsetMap[clone[i]];
      }

      // Copy into a new list while skipping eliminated cells.
      Transitions = clone.Where((t, i) => !skippedIndices.Contains(i)).ToList();

      // Create offsets.
      var offsets = new List<int>();
      for (var i = 0; i < builder.Transitions.Count; i += builder.AlphabetSize)
      {
        offsets.Add(offsetMap[i]);
      }
      Offsets = offsets;
    }

    /// <summary>
    /// The transitions after compaction.
    /// </summary>
    public IReadOnlyList<int> Transitions { get; }

    /// <summary>
    /// Mapping from stateId before compaction to offset of state after compaction.
    /// </summary>
    public IReadOnlyList<int> Offsets { get; }
  }
}