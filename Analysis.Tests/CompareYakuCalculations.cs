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

          var tiles = new List<Tile>();
          for (var i = 0; i < 34; i++)
          {
            var tileType = TileType.FromTileTypeId(i);
            for (var j = 0; j < tileCounts[i]; j++)
            {
              var tile = Tile.FromTileType(tileType, j);
              tiles.Add(tile);
            }
          }

          foreach (var tile in tiles.GroupBy(t => t.TileType))
          {
            var melds = new List<State.Meld>();
            var winningTile = tile.First();
            var roundWind = 0;
            var seatWind = 0;
            var hand = new HandCalculator();
            hand.Init(tiles.Select(t => t.TileType));

            var classicRon = ClassicYakuCalculator.Ron(winningTile, roundWind, seatWind, melds, tiles);
            var ron = YakuCalculator.Ron(hand, winningTile, roundWind, seatWind, melds);

            if (classicRon != ron)
            {

            }

            Assert.Equal(classicRon, ron);
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
  }
}