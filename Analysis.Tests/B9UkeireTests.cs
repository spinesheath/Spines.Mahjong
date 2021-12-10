using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.B9Ukeire;
using Spines.Mahjong.Analysis.Replay;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class B9UkeireTests
  {
    [Theory]
    [InlineData("123456789m123p11s", -1, "")]
    [InlineData("123456789m13p11s", 0, "2p")]
    [InlineData("468m1156778p345s", 1, "57m69p")]
    [InlineData("123459m34079p45s", 2, "1234569m789p3456s")]
    [InlineData("12558m3477p8s457z", 4, "12356789m23457p6789s457z")]
    public void SomeHands(string hand, int expectedShanten, string expectedUkeire)
    {
      var parser = new ShorthandParser(hand);

      var hashes = new int[4];
      foreach (var tile in parser.Tiles)
      {
        hashes[tile.SuitId] += Base5.Table[tile.Index];
      }

      var u = new UkeireCalculator(hashes, parser.Melds.Count());
      
      Assert.Equal(expectedShanten, u.Shanten);
      Assert.Equal(expectedUkeire, u.UkeireString);
    }

    [Fact]
    public void BundlesWithVisitor()
    {
      var files = Bundles.All.SelectMany(Directory.EnumerateFiles);
      var visitor = new UkeireComparingVisitor();
      foreach (var file in files)
      {
        ReplayParser.Parse(file, visitor);
      }

      Assert.Equal(0, visitor.ErrorCount);
    }
  }
}
