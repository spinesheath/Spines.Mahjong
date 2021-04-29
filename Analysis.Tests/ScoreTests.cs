using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class ScoreTests
  {
    [Fact]
    public void ParseBundles()
    {
      var sum = 0;

      var loadStatics = new HandCalculator();
      loadStatics.Init(Enumerable.Range(0, 13).Select(TileType.FromTileTypeId));
      sum += loadStatics.Shanten < 100 ? 0 : 1;

      var files = BundlesFolders.SelectMany(Directory.EnumerateFiles);
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        var r = ReplayParser.Parse(fileStream, false);
        sum += r;
      }

      Assert.Equal(1, sum);
    }

    private static readonly string[] BundlesFolders = 
    {
      @"C:\tenhou\compressed\2014\yonma\bundles",
      @"C:\tenhou\compressed\2015\yonma\bundles",
      @"C:\tenhou\compressed\2016\yonma\bundles"
    };
  }
}
