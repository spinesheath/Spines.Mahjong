using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  // TODO 111222333 can be iipeikou instead of sanankou from either ron or junchan. Add this information into the ID.
  // But since this shape requires 3 groups, it can not overlap with ssk, so maybe its possible to use the same bits for this information
  internal class FuFootprintCreator
  {
    public FuFootprintCreator(ArrangementGroup arrangements)
    {
      // Fu does not matter for chinitsu
      if (arrangements.TileCount == 14)
      {
        return;
      }

      var constraints = CreateConstraints(arrangements);

      var constraintToFu = new Dictionary<FuConstraint, int>();

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
          var pairIndex = arrangement.Blocks.FirstOrDefault(b => b.IsPair)?.Index;
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

        constraintToFu.Add(constraint, bestFu);
      }

      foreach (var keyValuePair in constraintToFu)
      {
        Footprint[keyValuePair.Key.Id] = (byte)keyValuePair.Value;
      }
    }

    public byte[] Footprint { get; } = new byte[680];

    private bool HasShuntsuWithWinningTile(Arrangement arrangement, int winningIndex)
    {
      return arrangement.Blocks.Any(b => b.IsShuntsu && b.Index <= winningIndex && b.Index + 2 >= winningIndex);
    }

    private static List<FuConstraint> CreateConstraints(ArrangementGroup arrangementGroup)
    {
      var uTypeIndex = arrangementGroup.UTypeIndex;
      var tileCounts = arrangementGroup.TileCounts;
      
      var constraints = new List<FuConstraint>();

      AddConstraints(constraints, -1, -1, -1);
      
      if (uTypeIndex >= 0)
      {
        AddConstraints(constraints, -1, uTypeIndex, -1);
        AddConstraints(constraints, -1, uTypeIndex + 1, -1);
        AddConstraints(constraints, -1, -1, uTypeIndex);
        AddConstraints(constraints, -1, -1, uTypeIndex + 3);
      }
      else if (arrangementGroup.TileCount < 9)
      {
        var shuntsus = arrangementGroup.Arrangements.SelectMany(a => a.Blocks.Where(b => b.IsShuntsu));
        var doujunIndexes = shuntsus.Select(s => s.Index).Distinct();
        foreach (var doujunIndex in doujunIndexes)
        {
          AddConstraints(constraints, -1, doujunIndex, -1);
        }

        var koustsus = arrangementGroup.Arrangements.SelectMany(a => a.Blocks.Where(b => b.IsKoutsu));
        var doukouIndexes = koustsus.Select(s => s.Index).Distinct();
        foreach (var doukouIndex in doukouIndexes)
        {
          AddConstraints(constraints, -1, -1, doukouIndex);
        }
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
          else if (arrangementGroup.TileCount < 9)
          {
            var shuntsus = arrangementGroup.Arrangements.SelectMany(a => a.Blocks.Where(b => b.IsShuntsu));
            var doujunIndexes = shuntsus.Select(s => s.Index).Distinct();
            foreach (var doujunIndex in doujunIndexes)
            {
              AddConstraints(constraints, winningIndex, doujunIndex, -1);
            }

            var koustsus = arrangementGroup.Arrangements.SelectMany(a => a.Blocks.Where(b => b.IsKoutsu));
            var doukouIndexes = koustsus.Select(s => s.Index).Distinct();
            foreach (var doukouIndex in doukouIndexes)
            {
              AddConstraints(constraints, winningIndex, -1, doukouIndex);
            }
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

    private class FuConstraint
    {
      public bool Open { get; }
      
      public bool Tsumo { get; }
      
      public int WinningIndex { get; }
      
      public int DoujunIndex { get; }

      public int DoukouIndex { get; }

      public int Id { get; }

      public FuConstraint(bool open, bool tsumo, int winningIndex, int doujunIndex, int doukouIndex)
      {
        Open = open;
        Tsumo = tsumo;
        WinningIndex = winningIndex;
        DoujunIndex = doujunIndex;
        DoukouIndex = doukouIndex;
        
        Id = doukouIndex + 1;
        if (doujunIndex >= 0)
        {
          Id += doujunIndex + 10;
        }

        Id *= 10;
        Id += winningIndex + 1;

        Id <<= 2;
        if (!tsumo)
        {
          Id |= 1;
        }

        if (open)
        {
          Id |= 2;
        }
      }
    }
  }
}