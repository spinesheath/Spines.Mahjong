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
      WideUTypeIndex = -1;
      Base5Hash = 0;
      TileCount = 0;
      TileCounts = new int[9];
      IipeikouIndex = -1;
      ExtraBlock = null;

      foreach (var arrangement in Arrangements)
      {
        TileCount = arrangement.TileCount;
        TileCounts = arrangement.TileCounts.ToArray();
        for (var i = 0; i < 6; i++)
        {
          if (arrangement.ContainsKoutsu(i) && arrangement.ContainsShuntsu(i + 1))
          {
            if (arrangement.ContainsPair(i + 3))
            {
              UTypeIndex = i;
              // Can't have identical koutsu. Identical shuntsu is handled with iipeikou below.
              ExtraBlock ??= arrangement.Blocks.FirstOrDefault(b => b.IsKoutsu && b.Index != i || b.IsShuntsu && b.Index != i + 1);
            }

            if (arrangement.ContainsShuntsu(i + 4) && arrangement.ContainsPair(i + 6))
            {
              WideUTypeIndex = i;
            }
          }
        }

        for (var i = 0; i < 7; i++)
        {
          if (arrangement.Blocks.Count(b => b.IsShuntsu && b.Index == i) == 2)
          {
            IipeikouIndex = i;
            ExtraBlock = arrangement.Blocks.First(b => b.IsShuntsu && b.Index == i);
          }
        }
      }

      // Fu does not matter for chinitsu
      if (TileCount == 14)
      {
        return;
      }

      HasUType = UTypeIndex >= 0 || WideUTypeIndex >= 0;
      if (!HasUType)
      {
        return;
      }

      int uTypeId;
      if (WideUTypeIndex >= 0)
      {
        uTypeId = 98 + WideUTypeIndex;
      }
      else if (TileCount == 8)
      {
        uTypeId = UTypeIndex;
      }
      else
      {
        var extraBlockIndex = ExtraBlock!.Index;
        if (ExtraBlock.IsKoutsu && extraBlockIndex > UTypeIndex + 2)
        {
          extraBlockIndex -= 1;
        }

        if (ExtraBlock.IsKoutsu && extraBlockIndex > UTypeIndex)
        {
          extraBlockIndex -= 1;
        }

        uTypeId = UTypeIndex + 6 * (1 + (ExtraBlock.IsShuntsu ? extraBlockIndex : extraBlockIndex + 7));
      }

      UTypeId = uTypeId;


      HasUType1 = false;
      HasUType9 = false;
      HasSquareType = false;
      SquareTypeIndex = 0L;
      foreach (var arrangement in Arrangements)
      {
        for (var i = 0; i < 7; i++)
        {
          if (arrangement.ContainsKoutsu(i) && arrangement.ContainsKoutsu(i + 1) && arrangement.ContainsKoutsu(i + 2))
          {
            HasSquareType = true;
            SquareTypeIndex = i;
          }
        }

        if (arrangement.ContainsKoutsu(0) && arrangement.ContainsShuntsu(1))
        {
          if (arrangement.ContainsPair(3))
          {
            HasUType1 = true;
          }

          if (arrangement.ContainsShuntsu(4) && arrangement.ContainsPair(6))
          {
            HasUType1 = true;
          }
        }
        else if (arrangement.ContainsKoutsu(8) && arrangement.ContainsShuntsu(5))
        {
          if (arrangement.ContainsPair(5))
          {
            HasUType9 = true;
          }

          if (arrangement.ContainsShuntsu(2) && arrangement.ContainsPair(2))
          {
            HasUType9 = true;
          }
        }
      }
    }

    public IEnumerable<Arrangement> Arrangements { get; }

    public int Base5Hash { get; }

    public Block? ExtraBlock { get; }

    public bool HasSquareType { get; }

    public bool HasUType { get; }

    public bool HasUType1 { get; }

    public bool HasUType9 { get; }

    public int IipeikouIndex { get; }

    public long SquareTypeIndex { get; }

    public int TileCount { get; }

    public int[] TileCounts { get; }

    public int UTypeId { get; }

    public int UTypeIndex { get; }

    public int WideUTypeIndex { get; }
  }
}