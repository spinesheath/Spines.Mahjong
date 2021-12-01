using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Score;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class ClassicScoreCalculatorTests
  {
    [Fact]
    public void BundlesWithClassicVisitor()
    {
      var files = Bundles.All.SelectMany(Directory.EnumerateFiles);
      var visitor = new ClassicScoreCalculatingVisitor();
      foreach (var file in files)
      {
        ReplayParser.Parse(file, visitor);
      }

      Assert.Equal(0, visitor.FailureCount);
    }

    [Theory]
    [InlineData("11122233399m111p", 0, 0, "1m", Yaku.Sanankou | Yaku.Toitoi)]
    [InlineData("11777888999m111p", 0, 0, "9m", Yaku.Sanankou | Yaku.Toitoi)]
    public void SomeHandsByRon(string handString, int roundWind, int seatWind, string discardString, Yaku expectedYaku)
    {
      var parser = new ShorthandParser(handString);
      var tiles = parser.Tiles.Select(t => Tile.FromTileType(t, 0)).ToList();
      var melds = new List<State.Meld>();
      foreach (var meld in parser.Melds)
      {
        var meldTiles = meld.Tiles.Select(t => Tile.FromTileType(t, 0)).ToList();
        if (meld.MeldId < 8)
        {
          melds.Add(State.Meld.Chii(meldTiles, meldTiles.First()));
        }
        else if (meld.MeldId < 7 + 9)
        {
          melds.Add(State.Meld.Pon(meldTiles, meldTiles.First()));
        }
        else
        {
          melds.Add(State.Meld.Ankan(meld.Tiles.First()));
        }
      }

      var winningTile = TileType.FromString(discardString);

      var (yaku, fu) = ClassicScoreCalculator.Ron(winningTile, roundWind, seatWind, melds, tiles);

      Assert.Equal(expectedYaku, yaku);
    }
  }
}