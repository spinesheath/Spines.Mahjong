using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class Arrangement
  {
    public Arrangement(IEnumerable<int> tileCounts, IEnumerable<Block> blocks)
    {
      Blocks = blocks.ToList();

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

    public int Base5Hash { get; }

    public IReadOnlyList<Block> Blocks { get; }

    public bool IsStandard => TileCount == 0 || Blocks.Count > 0;

    public int TileCount { get; }

    public IReadOnlyList<int> TileCounts { get; }

    public bool ContainsKoutsu(int index)
    {
      return Blocks.Any(b => b.IsKoutsu && b.Index == index);
    }

    public bool ContainsShuntsu(int index)
    {
      return Blocks.Any(b => b.IsShuntsu && b.Index == index);
    }

    public bool ContainsPair(int index)
    {
      return Blocks.Any(b => b.IsPair && b.Index == index);
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      for (var i = 0; i < TileCounts.Count; i++)
      {
        sb.Append((char) ('1' + i), TileCounts[i]);
      }

      sb.Append(':');
      sb.Append(string.Join(',', Blocks));

      return sb.ToString();
    }
  }
}