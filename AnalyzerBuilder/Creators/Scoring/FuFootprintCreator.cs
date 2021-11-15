using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class FuFootprintCreator
  {
    public FuFootprintCreator(ArrangementGroup arrangements)
    {
      // TODO exclude nonstandard hands

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
          if (!Matches(arrangements, arrangement, constraint))
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

    private static bool Matches(ArrangementGroup arrangements, Arrangement arrangement, FuConstraint constraint)
    {
      if (constraint.DoujunIndex >= 0 && !arrangement.ContainsShuntsu(constraint.DoujunIndex))
      {
        // open hands with less than 6 tiles in the arrangement can use a meld to get sanshoku
        if (arrangements.TileCount > 5 || !constraint.Open)
        {
          return false;
        }
      }

      if (constraint.DoukouIndex >= 0 && !arrangement.ContainsKoutsu(constraint.DoukouIndex))
      {
        // hands with less than 6 tiles in the arrangement can use a meld to get sanshoku. If closed must be ankan
        if (arrangements.TileCount > 5)
        {
          return false;
        }
      }
      
      var hasIipeikou = arrangement.Blocks.Where(b => b.IsShuntsu).GroupBy(s => s.Index).Any(g => g.Count() >= 2);
      if (arrangements.HasIipeikou && !constraint.Open && !hasIipeikou)
      {
        if (!arrangements.HasSquareType || constraint.SquareIsNotSanankou)
        {
          return false;
        }
      }

      if (constraint.SquareIsNotSanankou && arrangement.Blocks.Count(b => b.IsKoutsu) >= 3)
      {
        return false;
      }

      return true;
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

      for (var winningIndex = -1; winningIndex < 9; winningIndex++)
      {
        if (winningIndex == -1 || tileCounts[winningIndex] > 0)
        {
          AddConstraints(constraints, false, winningIndex, -1, -1);

          if (uTypeIndex >= 0)
          {
            AddConstraints(constraints, false, winningIndex, uTypeIndex, -1);
            AddConstraints(constraints, false, winningIndex, uTypeIndex + 1, -1);
            AddConstraints(constraints, false, winningIndex, -1, uTypeIndex);
            AddConstraints(constraints, false, winningIndex, -1, uTypeIndex + 3);
          }
          else if (arrangementGroup.TileCount < 6) // melds can be used for sanshoku here
          {
            // could have square in different suit, but not together with sanshoku
            AddConstraints(constraints, true, winningIndex, -1, -1);

            for (var i = 0; i < 7; i++)
            {
              AddConstraints(constraints, false, winningIndex, i, -1);
            }

            for (var i = 0; i < 9; i++)
            {
              if (tileCounts[i] < 2 || arrangementGroup.Arrangements.Any(a => a.ContainsKoutsu(i)))
              {
                AddConstraints(constraints, false, winningIndex, -1, i);
              }
            }
          }
          else if (arrangementGroup.TileCount < 9) // melds can't be used for sanshoku here
          {
            var shuntsus = arrangementGroup.Arrangements.SelectMany(a => a.Blocks.Where(b => b.IsShuntsu));
            var doujunIndexes = shuntsus.Select(s => s.Index).Distinct();
            foreach (var doujunIndex in doujunIndexes)
            {
              AddConstraints(constraints, false, winningIndex, doujunIndex, -1);
            }

            var koustsus = arrangementGroup.Arrangements.SelectMany(a => a.Blocks.Where(b => b.IsKoutsu));
            var doukouIndexes = koustsus.Select(s => s.Index).Distinct();
            foreach (var doukouIndex in doukouIndexes)
            {
              AddConstraints(constraints, false, winningIndex, -1, doukouIndex);
            }
          }
          else if (arrangementGroup.HasSquareType)
          {
            AddConstraints(constraints, true, winningIndex, -1, -1);
          }
        }
      }

      return constraints;
    }

    private static void AddConstraints(List<FuConstraint> constraints, bool squareIsNotSanankou, int winningIndex, int doujunIndex, int doukouIndex)
    {
      constraints.Add(new FuConstraint(false, false, squareIsNotSanankou, winningIndex, doujunIndex, doukouIndex));
      constraints.Add(new FuConstraint(false, true, squareIsNotSanankou, winningIndex, doujunIndex, doukouIndex));
      constraints.Add(new FuConstraint(true, false, squareIsNotSanankou, winningIndex, doujunIndex, doukouIndex));
      constraints.Add(new FuConstraint(true, true, squareIsNotSanankou, winningIndex, doujunIndex, doukouIndex));
    }

    private class FuConstraint
    {
      public bool Open { get; }
      
      public bool Tsumo { get; }
      
      public int WinningIndex { get; }
      
      public int DoujunIndex { get; }

      public int DoukouIndex { get; }

      /// <summary>
      /// True if 111222333 shape that is forced to not be interpreted as sanankou.
      /// </summary>
      public bool SquareIsNotSanankou { get; }

      public int Id { get; }

      public FuConstraint(bool open, bool tsumo, bool squareIsNotSanankou, int winningIndex, int doujunIndex, int doukouIndex)
      {
        Open = open;
        Tsumo = tsumo;
        SquareIsNotSanankou = squareIsNotSanankou;
        WinningIndex = winningIndex;
        DoujunIndex = doujunIndex;
        DoukouIndex = doukouIndex;
        
        Id = doukouIndex + 1;
        if (doujunIndex >= 0)
        {
          Id += doujunIndex + 10;
        }

        if (squareIsNotSanankou)
        {
          Id += 1;
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

      public override string ToString()
      {
        var open = Open ? "o" : "c";
        var tsumo = Tsumo ? "t" : "r";
        var win = WinningIndex == -1 ? "-" : WinningIndex.ToString();
        var doujun = DoujunIndex == -1 ? "-" : WinningIndex.ToString();
        var doukou = DoukouIndex == -1 ? "-" : WinningIndex.ToString();
        var square = SquareIsNotSanankou ? "s" : "k";
        return $"{Id}: {open}{tsumo}{win}{doujun}{doukou}{square}";
      }
    }
  }
}