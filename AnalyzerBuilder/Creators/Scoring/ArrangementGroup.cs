using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  /// <summary>
  /// Multiple arrangements of the same tiles.
  /// </summary>
  /// <remarks>
  /// u-type = 11123444 and 11123456777 shapes (guaranteed ankou, but lower value if wait on 1)
  /// </remarks>
  internal class ArrangementGroup
  {
    public ArrangementGroup(IEnumerable<Arrangement> arrangements)
    {
      Arrangements = arrangements.ToList();

      UTypeIndex = -1;
      var wideUTypeIndex = -1;
      Base5Hash = 0;
      TileCount = 0;
      TileCounts = new int[9];

      foreach (var arrangement in Arrangements)
      {
        Base5Hash = arrangement.Base5Hash;
        TileCount = arrangement.TileCount;
        TileCounts = arrangement.TileCounts.ToArray();
        for (var i = 0; i < 6; i++)
        {
          if (arrangement.ContainsKoutsu(i) && arrangement.ContainsShuntsu(i + 1))
          {
            if (arrangement.ContainsPair(i + 3))
            {
              UTypeIndex = i;
            }

            if (arrangement.ContainsShuntsu(i + 4) && arrangement.ContainsPair(i + 6))
            {
              wideUTypeIndex = i;
            }
          }
        }

        for (var i = 0; i < 7; i++)
        {
          if (arrangement.Blocks.Count(b => b.IsShuntsu && b.Index == i) == 2)
          {
            HasIipeikou = true;
          }
        }
      }

      // Fu does not matter for chinitsu
      if (TileCount == 14)
      {
        return;
      }

      HasUType = UTypeIndex >= 0 || wideUTypeIndex >= 0;
      
      HasSquareType = false;
      foreach (var arrangement in Arrangements)
      {
        for (var i = 0; i < 7; i++)
        {
          if (arrangement.ContainsKoutsu(i) && arrangement.ContainsKoutsu(i + 1) && arrangement.ContainsKoutsu(i + 2))
          {
            HasSquareType = true;
          }
        }
      }
    }

    public IEnumerable<Arrangement> Arrangements { get; }

    public int Base5Hash { get; }

    public bool HasSquareType { get; }

    public bool HasUType { get; }

    public bool HasIipeikou { get; }

    public int TileCount { get; }

    public int[] TileCounts { get; }

    public int UTypeIndex { get; }
  }
}