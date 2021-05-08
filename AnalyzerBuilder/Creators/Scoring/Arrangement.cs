using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal abstract class Arrangement
  {
    private protected Arrangement(IEnumerable<Block> blocks)
    {
      Blocks = blocks.ToList();
    }

    public IReadOnlyList<Block> Blocks { get; }

    public bool ContainsShuntsu(int index)
    {
      return Blocks.Any(b => b.IsShuntsu && b.Index == index);
    }

    public bool ContainsKoutsu(int index)
    {
      return Blocks.Any(b => b.IsKoutsu && b.Index == index);
    }
  }
}