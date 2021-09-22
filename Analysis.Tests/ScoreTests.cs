using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.Shanten;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class ScoreTests
  {
    [Fact]
    public void BundlesWithVisitor()
    {
      var files = BundlesFolders.SelectMany(Directory.EnumerateFiles);
      var visitor = new ScoreCalculatingVisitor();
      //foreach (var file in files.Take(500))
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        ReplayParser.Parse(fileStream, visitor);
      }

      Assert.Equal(0, visitor.FailureCount);
    }

    [Theory]
    [InlineData("789p111222333s44z", 0, 3, "4z", Yaku.Iipeikou | Yaku.ClosedChanta)]
    [InlineData("111m111789p999s44z", 0, 3, "4z", Yaku.ClosedChanta | Yaku.Sanankou)]
    [InlineData("111m111999s44z789P", 0, 3, "4z", Yaku.OpenChanta | Yaku.Sanankou)]
    [InlineData("111222333m111s44z", 0, 0, "1s", Yaku.Toitoihou | Yaku.Sanankou)]
    [InlineData("11m99p11778899s44z", 0, 0, "9s", Yaku.Chiitoitsu)]
    [InlineData("112233m55s2222M2222S", 0, 0, "2s", Yaku.Iipeikou)]
    [InlineData("111222333m234s44z", 0, 0, "2s", Yaku.Sanankou)]
    [InlineData("111222333m234s11z", 0, 0, "1m", Yaku.Iipeikou)]
    [InlineData("112233556677m11p", 0, 0, "2m", Yaku.Ryanpeikou)]
    [InlineData("111222333m12399p", 0, 0, "2p", Yaku.ClosedJunchan | Yaku.Iipeikou)]
    [InlineData("111222333m99p123P", 0, 0, "9p", Yaku.Sanankou)]
    [InlineData("123p11s789S555Z777Z", 0, 0, "2p", Yaku.Haku | Yaku.Chun | Yaku.OpenChanta)]
    [InlineData("789m789p11789s888M", 0, 0, "7s", Yaku.OpenSanshokuDoujun)]
    [InlineData("999m99p111999s123P", 0, 0, "9p", Yaku.Sanankou | Yaku.OpenJunchan)]
    [InlineData("123456789m12344p", 0, 0, "4p", Yaku.ClosedIttsuu)]
    [InlineData("123456789m44p123P", 0, 0, "4p", Yaku.OpenIttsuu)]
    [InlineData("123456m44p123P789M", 0, 0, "4p", Yaku.OpenIttsuu)]
    [InlineData("12344p123M456M789M", 0, 0, "4p", Yaku.OpenIttsuu)]
    [InlineData("11m123M1111P3333P5555P", 0, 0, "1m", Yaku.Sankantsu | Yaku.Sanankou)]
    [InlineData("11m1111P3333P5555P7777P", 0, 0, "1m", Yaku.Suukantsu | Yaku.SuuankouTanki)]
    [InlineData("222333444s66z666S", 0, 0, "2s", Yaku.Ryuuiisou)]
    [InlineData("88s234S234S666S666Z", 0, 0, "8s", Yaku.Ryuuiisou)]
    [InlineData("33344466s678M345P", 0, 0, "3s", Yaku.OpenTanyao)]
    public void SomeHandByRon(string handString, int roundWind, int seatWind, string discardString, Yaku expectedYaku)
    {
      var discard = Tile.FromTileType(TileType.FromString(discardString), 0);
      var sp = new ShorthandParser(handString);
      var hand = new HandCalculator(sp);

      var yaku = YakuCalculator.Ron(hand, discard, roundWind, seatWind, Melds(sp).ToList());

      Assert.Equal(expectedYaku, yaku);
    }

    private IEnumerable<State.Meld> Melds(ShorthandParser sp)
    {
      foreach (var meld in sp.Melds)
      {
        var tiles = meld.Tiles.Select(Tile.FromTileType).ToList();

        if (tiles.Count == 3)
        {
          if (tiles.GroupBy(t => t.TileType).Count() == 1)
          {
            yield return State.Meld.Pon(tiles, tiles[0]);
          }
          else
          {
            yield return State.Meld.Chii(tiles, tiles[0]);
          }
        }
        else
        {
          yield return State.Meld.Ankan(tiles[0].TileType);
        }
      }
    }

    [Fact]
    public void BundlesWithClassicVisitor()
    {
      var files = BundlesFolders.SelectMany(Directory.EnumerateFiles);
      var visitor = new ClassicScoreCalculatingVisitor();
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        ReplayParser.Parse(fileStream, visitor);
      }

      Assert.Equal(0, visitor.FailureCount);
      Assert.Equal(0, visitor.CalculationCount);
    }

    private static readonly string[] BundlesFolders = 
    {
      @"C:\tenhou\compressed\2014\yonma\bundles",
      @"C:\tenhou\compressed\2015\yonma\bundles",
      @"C:\tenhou\compressed\2016\yonma\bundles"
    };
  }
}
