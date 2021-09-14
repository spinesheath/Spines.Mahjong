using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.Shanten;
using Spines.Mahjong.Analysis.State;
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
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        ReplayParser.Parse(fileStream, visitor);
      }

      Assert.Equal(0, visitor.FailureCount);
      Assert.Equal(0, visitor.CalculationCount);
    }

    [Theory]
    [InlineData("789p111222333s44z", 0, 3, "4z", Yaku.Iipeikou | Yaku.ClosedChanta)]
    [InlineData("11m99p11778899s44z", 0, 0, "9s", Yaku.Chiitoitsu)]
    public void SomeHandByRon(string handString, int roundWind, int seatWind, string discardString, Yaku expectedYaku)
    {
      var discard = Tile.FromTileType(TileType.FromString(discardString), 0);
      var sp = new ShorthandParser(handString);
      var hand = new HandCalculator(sp);
      
      var yaku = YakuCalculator.Ron(hand, discard, roundWind, seatWind, new List<Meld>());

      Assert.Equal(expectedYaku, yaku);
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
