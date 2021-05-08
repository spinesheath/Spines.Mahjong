using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class ConcealedArrangement : Arrangement
  {
    public ConcealedArrangement(IEnumerable<int> tileCounts, IEnumerable<Block> blocks) 
      : base(blocks)
    {
      TileCounts = tileCounts.ToList();

      var h = 0;
      foreach (var c in TileCounts)
      {
        h *= 5;
        h += c;
        TileCount += c;
      }

      Base5Hash = h;
    }

    public int Base5Hash { get; }

    public int TileCount { get; }

    public IReadOnlyList<int> TileCounts { get; }
  }
}