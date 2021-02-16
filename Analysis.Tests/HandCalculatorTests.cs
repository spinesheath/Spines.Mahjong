using System.IO;
using System.Linq;
using System.Xml;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class HandCalculatorTests
  {
    [Fact]
    public void ParseBundles()
    {
      var sum = 0;

      var loadStatics = new HandCalculator();
      loadStatics.Init(Enumerable.Range(0, 13).Select(TileType.FromTileTypeId));
      sum += loadStatics.Shanten < 100 ? 0 : 1;

      var files = Directory.EnumerateFiles(BundlesFolder);
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        var r = ReplayParser.Parse(fileStream);
        sum += r;
      }

      Assert.Equal(1, sum);
    }

    [Fact]
    public void ParseCompressed()
    {
      var sum = 0;

      var loadStatics = new HandCalculator();
      loadStatics.Init(Enumerable.Range(0, 13).Select(TileType.FromTileTypeId));
      sum += loadStatics.Shanten < 100 ? 0 : 1;

      var files = Directory.EnumerateFiles(CompressedReplaysFolder);
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        var r = ReplayParser.Parse(fileStream);
        sum += r;
      }

      Assert.Equal(1, sum);
    }

    [Fact]
    public void ParseWithXmlReader()
    {
      var sum = 0;

      var loadStatics = new HandCalculator();
      loadStatics.Init(Enumerable.Range(0, 13).Select(TileType.FromTileTypeId));
      sum += loadStatics.Shanten < 100 ? 0 : 1;
      
      var xmlReaderSettings = new XmlReaderSettings { NameTable = null };

      var files = Directory.EnumerateFiles(ReplaysFolder).Take(60000);
      foreach (var file in files)
      {
        using var xmlReader = XmlReader.Create(file, xmlReaderSettings);
        var r = ReplayParser.Parse(xmlReader);
        sum += r;
      }

      Assert.Equal(1, sum);
    }

    [Theory]
    [InlineData("123456789m12344p", -1)]
    [InlineData("123456789m1234p", 0)]
    [InlineData("123456789m1245p", 1)]
    [InlineData("123456789m147p1s", 2)]
    [InlineData("12345679m147p14s", 3)]
    [InlineData("1345679m147p147s", 4)]
    [InlineData("145679m147p147s1z", 5)]
    [InlineData("14679m147p147s12z", 6)]
    [InlineData("1479m147p147s123z", 6)]
    [InlineData("147m147p147s1234z", 6)]
    [InlineData("123456789m44p123S", -1)]
    [InlineData("1245p112z333P6666P", 2)]
    [InlineData("123456789m44p111Z", -1)]
    [InlineData("1245p112z444Z3333Z", 1)]
    [InlineData("19m19p19s1234567z", 0)]
    [InlineData("114477m114477p11s", -1)]
    [InlineData("1111222445889s", 2)]
    public void JustSomeHands(string hand, int expected)
    {
      var parser = new ShorthandParser(hand);
      var c = new HandCalculator(parser);

      var actual = c.Shanten;

      Assert.Equal(expected, actual);
    }

    private const string ReplaysFolder = @"C:\tenhou\2014";
    private const string CompressedReplaysFolder = @"C:\tenhou\compressed\2014\yonma\actions";
    private const string BundlesFolder = @"C:\tenhou\compressed\2014\yonma\bundles";
  }
}
