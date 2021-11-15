using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Score;
using Meld = Spines.Mahjong.Analysis.State.Meld;

namespace EvaluatedWinningHandCreator
{
  internal class Program
  {
    private static void Main(string[] args)
    {
      WriteWinningHandScores("C:\\tenhou\\scoreDb");
    }

    private static void WriteWinningHandScores(string workingDirectory)
    {
      const int groupKinds = 34 + 21;
      const int maxGroupsHash = groupKinds * groupKinds * groupKinds * groupKinds;

      var pairs = new[]
      {
         0,  1,  2,  3,  4,  5,  6,  7,  8,
        18, 19, 20, 21, 22, 23, 24, 25, 26,
        27, 31, 32
      };
      foreach (var pair in pairs)
      {
        Console.WriteLine($"pair: {pair}");

        var path = Path.Combine(workingDirectory, $"standard{pair}.dat");
        using var fileStream = File.Create(path);
        using var binaryWriter = new BinaryWriter(fileStream);

        var total = 0;

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
            var melds = new List<Meld>();

            for (var i = 0; i < 4; i++)
            {
              var meldType = (m >> (2 * i)) & 3;
              if (meldType > 0)
              {
                melds.Add(GetMeld(k[i], meldType));
              }
              else
              {
                AddGroup(concealedTiles, k[i]);
              }
            }

            var tiles = new List<Tile>();
            for (var i = 0; i < 34; i++)
            {
              var tileType = TileType.FromTileTypeId(i);
              for (var j = 0; j < concealedTiles[i]; j++)
              {
                var tile = Tile.FromTileType(tileType, j);
                tiles.Add(tile);
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

              foreach (var (roundWind, seatWind) in windConfigurations)
              {
                var (tsumoYaku, tsumoFu) = ClassicScoreCalculator.Tsumo(winningTile, roundWind, seatWind, melds, tiles);
                var tsumoHan = Han.Calculate(tsumoYaku);

                binaryWriter.Write((byte)tsumoHan);
                if (tsumoHan < 5)
                {
                  binaryWriter.Write((byte)tsumoFu);
                }

                var (ronYaku, ronFu) = ClassicScoreCalculator.Ron(winningTile, roundWind, seatWind, melds, tiles);
                var ronHan = Han.Calculate(ronYaku);

                binaryWriter.Write((byte)ronHan);
                if (tsumoHan < 5)
                {
                  binaryWriter.Write((byte)ronFu);
                }

                total += 1;
                if (total % 1000000 == 0)
                {
                  var percentage = (double)groupsHash / maxGroupsHash;
                  Console.WriteLine($"{percentage:P} {total}");
                }
              }
            }
          }
        }
      }
    }

    private static readonly List<Tuple<int, int>> SimpleWindConfiguration = new()
    {
      Tuple.Create(0, 0)
    };

    private static readonly List<List<Tuple<int, int>>> WindConfigurations = new()
    {
      new List<Tuple<int, int>>
      {
        Tuple.Create(0, 0), // 4 fu
        Tuple.Create(0, 1), // 2 fu
        Tuple.Create(1, 1) // 0 fu
      },
      new List<Tuple<int, int>>
      {
        Tuple.Create(1, 1), // 4 fu
        Tuple.Create(0, 1), // 2 fu
        Tuple.Create(0, 0) // 0 fu
      },
      new List<Tuple<int, int>>
      {
        Tuple.Create(2, 2), // 4 fu
        Tuple.Create(0, 2), // 2 fu
        Tuple.Create(0, 0) // 0 fu
      },
      new List<Tuple<int, int>>
      {
        Tuple.Create(3, 3), // 4 fu
        Tuple.Create(0, 3), // 2 fu
        Tuple.Create(0, 0) // 0 fu
      }
    };


    private static Meld GetMeld(int kind, int meldType)
    {
      if (kind < 34)
      {
        if (meldType == 1)
        {
          var tiles = Enumerable.Range(4 * kind, 3).Select(Tile.FromTileId).ToList();
          return Meld.Pon(tiles, tiles.First());
        }

        if (meldType == 2)
        {
          return Meld.Ankan(TileType.FromTileTypeId(kind));
        }

        return Meld.Daiminkan(Tile.FromTileId(4 * kind));
      }

      {
        var x = kind - 34;
        var suit = x / 7;
        var index = x % 7;
        var tiles = Enumerable.Range(index, 3).Select(i => 4 * (9 * suit + i)).Select(Tile.FromTileId).ToList();
        return Meld.Chii(tiles, tiles.First());
      }
    }

    private static void AddGroup(IList<int> tileCounts, int kind)
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