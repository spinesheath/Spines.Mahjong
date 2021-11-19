using System;
using System.Collections.Generic;
using System.IO;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.Shanten;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class CompareYakuCalculations
  {
    [Fact]
    public void CompareWithEvaluatedHands()
    {
      const string workingDirectory = "C:\\tenhou\\scoreDb";
      const int numberOfGroupKinds = 34 + 21;
      const int maxGroupsHash = numberOfGroupKinds * numberOfGroupKinds * numberOfGroupKinds * numberOfGroupKinds;
      var failureCount = 0;

      var pairTileTypeIds = new[]
      {
         0, 1, 2, 
        //3,  4,  5,  6,  7,  8,
        //18, 19, 20, 21, 22, 23, 24, 25, 26,
        //27, 31, 32
      };

      var tileCounts = new int[34];
      var groupKinds = new int[4];
      var base5Hashes = new int[4];
      Span<int> base5Table = stackalloc int[9];
      Base5.Table.CopyTo(base5Table);
      var scoringData = new ProgressiveScoringData();
      foreach (var pairTileTypeId in pairTileTypeIds)
      {
        var path = Path.Combine(workingDirectory, $"standard{pairTileTypeId}.dat");
        using var fileStream = File.OpenRead(path);
        using var binaryReader = new BinaryReader(fileStream);

        var windConfigurations = SimpleWindConfiguration;
        if (pairTileTypeId >= 27 && pairTileTypeId < 31)
        {
          windConfigurations = WindConfigurations[pairTileTypeId - 27];
        }

        for (var groupsHash = 0; groupsHash < maxGroupsHash; groupsHash++)
        {
          Array.Clear(tileCounts, 0, tileCounts.Length);
          tileCounts[pairTileTypeId] += 2;

          var g = groupsHash;
          groupKinds[0] = g % numberOfGroupKinds;
          g /= numberOfGroupKinds;
          groupKinds[1] = g % numberOfGroupKinds;
          g /= numberOfGroupKinds;
          groupKinds[2] = g % numberOfGroupKinds;
          g /= numberOfGroupKinds;
          groupKinds[3] = g;

          if (groupKinds[0] > groupKinds[1] || groupKinds[1] > groupKinds[2] || groupKinds[2] > groupKinds[3])
          {
            continue;
          }

          AddGroup(tileCounts, groupKinds[0]);
          AddGroup(tileCounts, groupKinds[1]);
          AddGroup(tileCounts, groupKinds[2]);
          AddGroup(tileCounts, groupKinds[3]);

          if (InvalidTileCounts(tileCounts))
          {
            continue;
          }

          var invalidKanFlags = InvalidKanFlags(groupKinds, tileCounts);

          for (var groupInterpretationIterator = 0; groupInterpretationIterator < 256; groupInterpretationIterator++)
          {
            if ((groupInterpretationIterator & invalidKanFlags) != 0)
            {
              continue;
            }

            base5Hashes[0] = 0;
            base5Hashes[1] = 0;
            base5Hashes[2] = 0;
            base5Hashes[3] = 0;
            var tilePresences = 1L << pairTileTypeId;

            base5Hashes[pairTileTypeId / 9] += base5Table[pairTileTypeId % 9] * 2;

            for (var i = 0; i < 4; i++)
            {
              var meldType = (groupInterpretationIterator >> (2 * i)) & 3;
              if (meldType <= 0)
              {
                var kind = groupKinds[i];
                if (kind < 34)
                {
                  base5Hashes[kind / 9] += base5Table[kind % 9] * 3;

                  tilePresences |= 1L << kind;
                }
                else
                {
                  var x = kind - 34;
                  var suit = x / 7;
                  var index = x % 7;
                  base5Hashes[suit] += base5Table[index] * 31;

                  tilePresences |= 0b111L << (9 * suit + index);
                }
              }
            }

            var data = CreateProgressiveScoringData(scoringData, groupKinds, base5Hashes, groupInterpretationIterator);

            for (var i = 0; i < 34; i++)
            {
              if ((tilePresences >> i & 1L) == 0)
              {
                continue;
              }

              var winningTile = TileType.FromTileTypeId(i);

              for (var index = 0; index < windConfigurations.Length; index++)
              {
                var wind = windConfigurations[index];
                var (tsumoHan, tsumoFu) = ScoreCalculator.Tsumo(data, wind, winningTile);

                var expectedTsumoHan = binaryReader.ReadByte();
                if (expectedTsumoHan != tsumoHan)
                {
                  failureCount += 1;
                }

                if (expectedTsumoHan < 5)
                {
                  int expectedTsumoFu = binaryReader.ReadByte();
                  if (expectedTsumoFu != tsumoFu)
                  {
                    failureCount += 1;
                  }
                }

                var (ronHan, ronFu) = ScoreCalculator.Ron(data, wind, winningTile);

                var expectedRonHan = binaryReader.ReadByte();
                if (expectedRonHan != ronHan)
                {
                  failureCount += 1;
                }

                if (expectedTsumoHan < 5)
                {
                  int expectedRonFu = binaryReader.ReadByte();
                  if (expectedRonFu != ronFu)
                  {
                    failureCount += 1;
                  }
                }
              }
            }
          }
        }
      }

      Assert.Equal(0, failureCount);
    }

    private static bool InvalidTileCounts(int[] tileCounts)
    {
      for (var i = 0; i < tileCounts.Length; i++)
      {
        if (tileCounts[i] > 4)
        {
          return true;
        }
      }

      return false;
    }

    private static int InvalidKanFlags(int[] groupKinds, int[] tileCounts)
    {
      var invalidKanFlags = 0;
      for (var i = 0; i < 4; i++)
      {
        // Shuntsu can not be kan. No free tile can not be kan
        if (groupKinds[i] >= 34 || tileCounts[groupKinds[i]] == 4)
        {
          invalidKanFlags |= 2 << (i * 2);
        }
      }

      return invalidKanFlags;
    }

    private static ProgressiveScoringData CreateProgressiveScoringData(ProgressiveScoringData data, int[] groupKinds, int[] base5Hashes, int groupInterpretationIterator)
    {
      data.Reset();
      
      for (var i = 0; i < 4; i++)
      {
        var meldType = (groupInterpretationIterator >> (2 * i)) & 3;
        if (meldType == 0)
        {
          continue;
        }
        
        var kind = groupKinds[i];

        if (kind < 34)
        {
          var suitId = kind / 9;
          var index = kind % 9;
          if (meldType == 1)
          {
            data.Pon(suitId, index);
          }
          else if (meldType == 2)
          {
            data.Ankan(suitId, index);
          }
          else
          {
            data.Daiminkan(suitId, index);
          }
        }
        else
        {
          var x = kind - 34;
          var suitId = x / 7;
          var meldId = x % 7;
          data.Chii(suitId, meldId);
        }
      }

      data.Init(base5Hashes);

      return data;
    }

    private static readonly WindScoringData[] SimpleWindConfiguration = { new(0, 0) };

    /// <summary>
    /// 0, 2, 4 fu for each wind pair
    /// </summary>
    private static readonly List<WindScoringData[]> WindConfigurations = new()
    {
      new WindScoringData[] { new(0, 0), new(0, 1), new(1, 1) },
      new WindScoringData[] { new(1, 1), new(0, 1), new(0, 0) },
      new WindScoringData[] { new(2, 2), new(0, 2), new(0, 0) },
      new WindScoringData[] { new(3, 3), new(0, 3), new(0, 0) },
    };

    private static void AddGroup(int[] tileCounts, int kind)
    {
      if (kind < 34)
      {
        tileCounts[kind] += 3;
      }
      else
      {
        var x = kind - 34;
        var suit = x / 7;
        var index = x % 7;
        tileCounts[9 * suit + index + 0] += 1;
        tileCounts[9 * suit + index + 1] += 1;
        tileCounts[9 * suit + index + 2] += 1;
      }
    }
  }
}