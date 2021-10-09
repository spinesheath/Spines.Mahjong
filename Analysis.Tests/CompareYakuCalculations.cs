using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.Shanten;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class CompareYakuCalculations
  {
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
              var (yaku, fu) = YakuCalculator.Tsumo(hand, winningTile, roundWind, seatWind);

              if (classicYaku != yaku)
              {
              }

              Assert.Equal(classicYaku, yaku);
            }
          }
        }
      }
    }

    private void AddGroup(int[] tileCounts, int kind)
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

    private void AddMeld(List<State.Meld> melds, List<int>[] meldIds, int kind)
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