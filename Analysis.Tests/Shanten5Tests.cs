using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten5;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class Shanten5Tests
  {
    [Fact]
    public void BundlesWithVisitor()
    {
      var files = BundlesFolders.SelectMany(Directory.EnumerateFiles);
      var visitor = new Shanten5EvaluatingVisitor();
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        ReplayParser.Parse(fileStream, visitor);
      }

      Assert.Equal(0, visitor.ErrorCount);
      Assert.Equal(1, visitor.EvaluationCount);
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
    [InlineData("123456789m44p123S", -1)]
    [InlineData("1245p112z333P6666P", 1)]
    [InlineData("123456789m44p111Z", -1)]
    [InlineData("1245p112z444Z3333Z", 1)]
    [InlineData("19m19p19s1234567z", 0)]
    [InlineData("19m19p19s12345677z", -1)]
    [InlineData("19m19p19s12346677z", 0)]
    [InlineData("19m19p19s1234677z", 0)]
    [InlineData("19m19p19s1256677z", 1)]
    [InlineData("159m19p19s125677z", 1)]
    [InlineData("1111222445889s", 2)]
    [InlineData("77z1111Z2222Z3333Z4444Z", -1)]
    [InlineData("114477m114477p11s", -1)]
    [InlineData("114477m114477p1s", 0)]
    [InlineData("114477m11447p14s", 1)]
    [InlineData("114477m1147p147s", 2)]
    [InlineData("114477m147p147s1z", 3)]
    [InlineData("11447m147p147s12z", 4)]
    [InlineData("1147m147p147s123z", 5)]
    [InlineData("147m147p147s1234z", 6)]
    [InlineData("115599m1155p111S", 2)]
    [InlineData("115599m1155p111Z", 2)]
    [InlineData("115599m1155p1111Z", 2)]
    [InlineData("268m333678p33s77z", 1)]
    [InlineData("268m333678p33s777Z", 0)]
    [InlineData("11369m139p1s12234z", 3)]
    public void JustSomeHand(string hand, int expected)
    {
      var parser = new ShorthandParser(hand);
      var counts = parser.Concealed.ToArray();

      var s = new Shanten5Calculator().Calculate(counts, (byte)parser.Melds.Count());

      Assert.Equal(expected, s);
    }

    private static readonly string[] BundlesFolders = 
    {
      @"C:\tenhou\compressed\2014\yonma\bundles",
      @"C:\tenhou\compressed\2015\yonma\bundles",
      @"C:\tenhou\compressed\2016\yonma\bundles"
    };
  }
}