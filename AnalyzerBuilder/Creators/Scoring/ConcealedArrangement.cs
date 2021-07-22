using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class ConcealedArrangement : Arrangement
  {
    public ConcealedArrangement(IEnumerable<int> tileCounts, IEnumerable<Block> blocks) 
      : base(blocks)
    {
      TileCounts = tileCounts.ToList();

      var h = 0;
      foreach (var c in TileCounts.Reverse())
      {
        h *= 5;
        h += c;
        TileCount += c;
      }

      Base5Hash = h;
    }

    public bool IsStandard => TileCount == 0 || Blocks.Count > 0;

    public int Base5Hash { get; }

    public int TileCount { get; }

    public IReadOnlyList<int> TileCounts { get; }

    public override string ToString()
    {
      var sb = new StringBuilder();
      for (var i = 0; i < TileCounts.Count; i++)
      {
        sb.Append((char)('1' + i), TileCounts[i]);
      }

      sb.Append(':');
      sb.Append(string.Join(',', Blocks));

      return sb.ToString();
    }
  }
}