using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.Shanten;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class ClassicYakuCalculatorTests
  {
    [Theory]
    [InlineData("11122233399m111p", 0, 0, "1m", Yaku.Sanankou | Yaku.Toitoihou)]
    [InlineData("11777888999m111p", 0, 0, "9m", Yaku.Sanankou | Yaku.Toitoihou)]
    public void SomeHandsByRon(string handString, int roundWind, int seatWind, string discardString, Yaku expectedYaku)
    {
      var parser = new ShorthandParser(handString);
      var tiles = parser.Tiles.Select(t => Tile.FromTileType(t, 0)).ToList();
      var melds = new List<State.Meld>();
      var winningTile = Tile.FromTileType(TileType.FromString(discardString), 0);

      var classicRon = ClassicYakuCalculator.Ron(winningTile, roundWind, seatWind, melds, tiles);

      Assert.Equal(expectedYaku, classicRon);
    }
  }
}