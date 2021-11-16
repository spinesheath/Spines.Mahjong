using System.Collections.Generic;
using System.IO;
using System.Linq;
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
      const int groupKinds = 34 + 21;
      const int maxGroupsHash = groupKinds * groupKinds * groupKinds * groupKinds;
      var failureCount = 0;

      var pairs = new[]
      {
         0,  
        // 1,  2,  3,  4,  5,  6,  7,  8,
        //18, 19, 20, 21, 22, 23, 24, 25, 26,
        //27, 31, 32
      };

      foreach (var pair in pairs)
      {
        var path = Path.Combine(workingDirectory, $"standard{pair}.dat");
        using var fileStream = File.OpenRead(path);
        using var binaryReader = new BinaryReader(fileStream);
        
        for (var groupsHash = 0; groupsHash < maxGroupsHash; groupsHash++)
        {
          var tileCounts = new int[34];
          tileCounts[pair] += 2;
          var k = new int[4];

          var g = groupsHash;
          k[0] = g % groupKinds;
          g /= groupKinds;
          k[1] = g % groupKinds;
          g /= groupKinds;
          k[2] = g % groupKinds;
          g /= groupKinds;
          k[3] = g;

          if (k[0] > k[1] || k[1] > k[2] || k[2] > k[3])
          {
            continue;
          }

          AddGroup(tileCounts, k[0]);
          AddGroup(tileCounts, k[1]);
          AddGroup(tileCounts, k[2]);
          AddGroup(tileCounts, k[3]);

          if (tileCounts.Any(c => c > 4))
          {
            continue;
          }

          var invalidKanFlags = 0;
          for (var i = 0; i < 4; i++)
          {
            // Shuntsu can not be kan. No free tile can not be kan
            if (k[i] >= 34 || tileCounts[k[i]] == 4)
            {
              invalidKanFlags |= 2 << (i * 2);
            }
          }

          for (var m = 0; m < 256; m++)
          {
            if ((m & invalidKanFlags) != 0)
            {
              continue;
            }

            var concealedTiles = new int[34];
            concealedTiles[pair] += 2;
            
            for (var i = 0; i < 4; i++)
            {
              var meldType = (m >> (2 * i)) & 3;
              if (meldType <= 0)
              {
                AddGroup(concealedTiles, k[i]);
              }
            }

            var base5Hashes = new int[4];
            for (var i = 0; i < 34; i++)
            {
              var suit = i / 9;
              var index = i % 9;
              base5Hashes[suit] += concealedTiles[i] * Base5.Table[index];
            }

            var data = new ProgressiveScoringData();
            data.Init(base5Hashes);

            for (var i = 0; i < 4; i++)
            {
              var meldType = (m >> (2 * i)) & 3;
              if (meldType > 0)
              {
                var (suitId, meldId) = GetSuitAndMeldId(k[i], meldType);

                if (meldId < 7)
                {
                  data.Chii(suitId, meldId, base5Hashes[suitId]);
                }
                else if (meldId < 16)
                {
                  data.Pon(suitId, meldId - 7, base5Hashes[suitId]);
                }
                else if (meldId < 25)
                {
                  data.Ankan(suitId, meldId - 16, base5Hashes[suitId]);
                }
                else
                {
                  data.Daiminkan(suitId, meldId - 25, base5Hashes[suitId]);
                }
              }
            }

            for (var i = 0; i < 34; i++)
            {
              if (concealedTiles[i] == 0)
              {
                continue;
              }

              var winningTile = TileType.FromTileTypeId(i);

              var windConfigurations = SimpleWindConfiguration;
              if (pair >= 27 && pair < 31)
              {
                windConfigurations = WindConfigurations[pair - 27];
              }

              foreach (var wind in windConfigurations)
              {
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

    private static readonly List<IWindScoringData> SimpleWindConfiguration = new()
    {
      new WindScoringData(0, 0)
    };

    private static readonly List<List<IWindScoringData>> WindConfigurations = new()
    {
      new List<IWindScoringData>
      {
        new WindScoringData(0, 0), // 4 fu
        new WindScoringData(0, 1), // 2 fu
        new WindScoringData(1, 1) // 0 fu
      },
      new List<IWindScoringData>
      {
        new WindScoringData(1, 1), // 4 fu
        new WindScoringData(0, 1), // 2 fu
        new WindScoringData(0, 0) // 0 fu
      },
      new List<IWindScoringData>
      {
        new WindScoringData(2, 2), // 4 fu
        new WindScoringData(0, 2), // 2 fu
        new WindScoringData(0, 0) // 0 fu
      },
      new List<IWindScoringData>
      {
        new WindScoringData(3, 3), // 4 fu
        new WindScoringData(0, 3), // 2 fu
        new WindScoringData(0, 0) // 0 fu
      },
    };
    
    private static (int, int) GetSuitAndMeldId(int kind, int meldType)
    {
      if (kind < 34)
      {
        var suit = kind / 9;
        var index = kind % 9;
        return (suit, 9 * meldType - 2 + index);
      }

      {
        var x = kind - 34;
        var suit = x / 7;
        var index = x % 7;
        return (suit, index);
      }
    }

    [Fact]
    public void Regular()
    {
      var groupKinds = 34 + 21;
      var maxGroupsHash = groupKinds * groupKinds * groupKinds * groupKinds;
      for (var pair = 0; pair < 34; pair++)
      {
        for (var groupsHash = 0; groupsHash < maxGroupsHash; groupsHash++)
        {
          var tileCounts = new int[34];
          tileCounts[pair] += 2;

          var g = groupsHash;
          var k0 = g % groupKinds;
          g /= groupKinds;
          var k1 = g % groupKinds;
          g /= groupKinds;
          var k2 = g % groupKinds;
          g /= groupKinds;
          var k3 = g;

          if (k0 > k1 || k1 > k2 || k2 > k3)
          {
            continue;
          }

          AddGroup(tileCounts, k0);
          AddGroup(tileCounts, k1);
          AddGroup(tileCounts, k2);
          AddGroup(tileCounts, k3);

          if (tileCounts.Any(c => c > 4))
          {
            continue;
          }

          for (var m = 0; m < 16; m++)
          {
            var concealedTiles = new int[34];
            concealedTiles[pair] += 2;
            var melds = new List<State.Meld>();
            var meldIds = new[] {new List<int>(), new List<int>(), new List<int>(), new List<int>()};
            if ((m & 1) > 0)
            {
              AddMeld(melds, meldIds, k0);
            }
            else
            {
              AddGroup(concealedTiles, k0);
            }

            if ((m & 2) > 0)
            {
              AddMeld(melds, meldIds, k1);
            }
            else
            {
              AddGroup(concealedTiles, k1);
            }

            if ((m & 4) > 0)
            {
              AddMeld(melds, meldIds, k2);
            }
            else
            {
              AddGroup(concealedTiles, k2);
            }

            if ((m & 8) > 0)
            {
              AddMeld(melds, meldIds, k3);
            }
            else
            {
              AddGroup(concealedTiles, k3);
            }

            var tiles = new List<Tile>();
            var tileTypes = new List<TileType>();
            for (var i = 0; i < 34; i++)
            {
              var tileType = TileType.FromTileTypeId(i);
              for (var j = 0; j < concealedTiles[i]; j++)
              {
                var tile = Tile.FromTileType(tileType, j);
                tiles.Add(tile);
                tileTypes.Add(tileType);
              }
            }

            foreach (var tile in tiles.GroupBy(t => t.TileType))
            {
              var winningTile = tile.Key;
              var roundWind = 0;
              var seatWind = 0;
              var hand = new HandCalculator(tileTypes, meldIds[0], meldIds[1], meldIds[2], meldIds[3]);

              var (classicYaku, classicFu) = ClassicScoreCalculator.Tsumo(winningTile, roundWind, seatWind, melds, tiles);

              var wind = new WindScoringData(roundWind, seatWind);
              var (yaku, fu) = ScoreCalculator.TsumoWithYaku(hand.ScoringData, wind, winningTile);

              if (classicYaku != yaku)
              {
              }

              var han = Han.Calculate(yaku);
              if (han > 4 || han == 4 && classicFu >= 40 && fu >= 40)
              {
                continue;
              }

              if (classicFu != fu)
              {
              }

              Assert.Equal(classicYaku, yaku);
            }
          }
        }
      }
    }

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

    private static void AddMeld(List<State.Meld> melds, List<int>[] meldIds, int kind)
    {
      if (kind < 34)
      {
        var tiles = Enumerable.Range(4 * kind, 3).Select(Tile.FromTileId).ToList();
        melds.Add(State.Meld.Pon(tiles, tiles.First()));
        var suit = kind / 9;
        var index = kind % 9;
        meldIds[suit].Add(7 + index);
      }
      else
      {
        var x = kind - 34;
        var suit = x / 7;
        var index = x % 7;
        var tiles = Enumerable.Range(index, 3).Select(i => 4 * (9 * suit + i)).Select(Tile.FromTileId).ToList();
        melds.Add(State.Meld.Chii(tiles, tiles.First()));
        meldIds[suit].Add(index);
      }
    }
  }
}