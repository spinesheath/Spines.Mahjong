using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class UTypeFuCreator
  {
    public UTypeFuCreator(ArrangementGroup arrangements)
    {
      // TODO ankouFu from extra group?

      // Fu does not matter for chinitsu
      if (arrangements.TileCount == 14)
      {
        return;
      }

      HasUType = arrangements.HasUType;
      if (!arrangements.HasUType)
      {
        return;
      }

      var uTypeId = arrangements.UTypeId;
      
      var constraints = CreateConstraints(arrangements.UTypeIndex, arrangements.TileCounts);

      foreach (var constraint in constraints)
      {
        var bestFu = 0;

        foreach (var arrangement in arrangements.Arrangements)
        {
          if (constraint.DoujunIndex >= 0 && !arrangement.ContainsShuntsu(constraint.DoujunIndex))
          {
            continue;
          }

          if (constraint.DoukouIndex >= 0 && !arrangement.ContainsKoutsu(constraint.DoukouIndex))
          {
            continue;
          }

          if (arrangements.IipeikouIndex >= 0 && !constraint.Open && arrangement.Blocks.Count(b => b.IsShuntsu && b.Index == arrangements.IipeikouIndex) != 2)
          {
            continue;
          }

          var winningIndex = constraint.WinningIndex;
          var pairIndex = arrangement.Blocks.First(b => b.IsPair).Index;
          var shuntsus = arrangement.Blocks.Where(b => b.IsShuntsu).ToList();
          var fu = 0;

          foreach (var koutsu in arrangement.Blocks.Where(b => b.IsKoutsu))
          {
            if (constraint.Tsumo || koutsu.Index != winningIndex || HasShuntsuWithWinningTile(arrangement, winningIndex))
            {
              fu += koutsu.IsJunchanBlock ? 8 : 4;
            }
            else
            {
              fu += koutsu.IsJunchanBlock ? 4 : 2;
            }
          }

          if (pairIndex == winningIndex)
          {
            fu += 2;
          }
          else if (shuntsus.Any(s => s.Index == winningIndex - 1))
          {
            fu += 2;
          }
          else if (shuntsus.Any(s => s.Index == 0 && winningIndex == 2))
          {
            fu += 2;
          }
          else if (shuntsus.Any(s => s.Index == 6 && winningIndex == 6))
          {
            fu += 2;
          }

          bestFu = Math.Max(bestFu, fu);
        }

        var patternAndConstraintId = constraint.Key + (uTypeId << 13);
        KeyToFu.Add(patternAndConstraintId, bestFu);
      }

      // TODO wrongly calculated single wait fu
      //foreach (var block in arrangement.Blocks)
      //{
      //  if (block.IsPair)
      //  {
      //    WaitShiftValue |= 1L << (offset + 1 + block.Index);
      //  }
      //  else if (block.IsShuntsu)
      //  {
      //    WaitShiftValue |= 1L << (offset + 1 + block.Index + 1);

      //    if (block.Index == 0)
      //    {
      //      WaitShiftValue |= 1L << (offset + 1 + 2);
      //    }

      //    if (block.Index == 6)
      //    {
      //      WaitShiftValue |= 1L << (offset + 1 + 6);
      //    }
      //  }
      //}
    }

    public Dictionary<int, int> KeyToFu = new Dictionary<int, int>();

    private bool HasShuntsuWithWinningTile(Arrangement arrangement, int winningIndex)
    {
      return arrangement.Blocks.Any(b => b.IsShuntsu && b.Index <= winningIndex && b.Index + 2 >= winningIndex);
    }

    private static List<FuConstraint> CreateConstraints(int uTypeIndex, int[] tileCounts)
    {
      var constraints = new List<FuConstraint>();

      AddConstraints(constraints, -1, -1, -1);
      
      if (uTypeIndex >= 0)
      {
        AddConstraints(constraints, -1, uTypeIndex, -1);
        AddConstraints(constraints, -1, uTypeIndex + 1, -1);
        AddConstraints(constraints, -1, -1, uTypeIndex);
        AddConstraints(constraints, -1, -1, uTypeIndex + 3);
      }

      for (var winningIndex = 0; winningIndex < 9; winningIndex++)
      {
        if (tileCounts[winningIndex] > 0)
        {
          AddConstraints(constraints, winningIndex, -1, -1);

          if (uTypeIndex >= 0)
          {
            AddConstraints(constraints, winningIndex, uTypeIndex, -1);
            AddConstraints(constraints, winningIndex, uTypeIndex + 1, -1);
            AddConstraints(constraints, winningIndex, -1, uTypeIndex);
            AddConstraints(constraints, winningIndex, -1, uTypeIndex + 3);
          }
        }
      }

      return constraints;
    }

    private static void AddConstraints(List<FuConstraint> constraints, int winningIndex, int doujunIndex, int doukouIndex)
    {
      constraints.Add(new FuConstraint(false, false, winningIndex, doujunIndex, doukouIndex));
      constraints.Add(new FuConstraint(false, true, winningIndex, doujunIndex, doukouIndex));
      constraints.Add(new FuConstraint(true, false, winningIndex, doujunIndex, doukouIndex));
      constraints.Add(new FuConstraint(true, true, winningIndex, doujunIndex, doukouIndex));
    }

    public bool HasUType { get; }

    private class FuConstraint
    {
      public bool Open { get; }
      
      public bool Tsumo { get; }
      
      public int WinningIndex { get; }
      
      public int DoujunIndex { get; }

      public int DoukouIndex { get; }

      public int Key { get; }

      public FuConstraint(bool open, bool tsumo, int winningIndex, int doujunIndex, int doukouIndex)
      {
        Open = open;
        Tsumo = tsumo;
        WinningIndex = winningIndex;
        DoujunIndex = doujunIndex;
        DoukouIndex = doukouIndex;

        Key = winningIndex + 1;
        
        if (tsumo)
        {
          Key |= 1 << 4;
        }

        if (open)
        {
          Key |= 1 << 5;
        }

        if (doujunIndex >= 0)
        {
          var openFactor = open ? 2 : 1;
          Key |= (openFactor * (4 + doujunIndex)) << 6;
        }

        if (doukouIndex >= 0)
        {
          Key |= ((9 + doukouIndex) * 4) << 6;
        }
      }
    }
  }
}