using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Combinations
{
  /// <summary>
  /// A combination of tiles.
  /// </summary>
  public class Combination
  {
    /// <summary>
    /// Constructs a new Combination for a given number of types.
    /// </summary>
    public Combination(IEnumerable<int> counts)
    {
      Counts = counts.ToList();
    }

    /// <summary>
    /// The counts for each tile type in the Combination.
    /// </summary>
    public IReadOnlyCollection<int> Counts { get; }
  }
}