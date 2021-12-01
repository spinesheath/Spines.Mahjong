using System;
using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class HandCalculatorTests
  {
    static HandCalculatorTests()
    {
      var loadStatics = new HandCalculator();
      loadStatics.Init(Enumerable.Range(0, 13).Select(TileType.FromTileTypeId));
      Console.WriteLine(loadStatics.Shanten);
    }

    [Fact]
    public void BundlesWithVisitor()
    {
      var files = Bundles.All.SelectMany(Directory.EnumerateFiles);
      var visitor = new ShantenEvaluatingVisitor();
      foreach (var file in files)
      {
        ReplayParser.Parse(file, visitor);
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
    [InlineData("1245p112z333P6666P", 2)]
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
    public void JustSomeHands(string hand, int expected)
    {
      var parser = new ShorthandParser(hand);
      var c = new HandCalculator(parser);

      var actual = c.Shanten;

      Assert.Equal(expected, actual);
    }

    [Fact]
    public void ChiitoiDiscard()
    {
      var parser = new ShorthandParser("114477m114477p11s");
      var c = new HandCalculator(parser);
      c.Discard(TileType.FromSuitAndIndex(Suit.Manzu, 0));
      c.Draw(TileType.FromSuitAndIndex(Suit.Jihai, 6));
      c.Discard(TileType.FromSuitAndIndex(Suit.Manzu, 3));

      var actual = c.Shanten;

      Assert.Equal(1, actual);
    }

    [Fact]
    public void BlockingAnkan()
    {
      var parser = new ShorthandParser("12223m11222333z");
      var c = new HandCalculator(parser);
      var m2 = TileType.FromSuitAndIndex(Suit.Manzu, 1);
      c.Draw(m2);
      c.Ankan(m2);

      var actual = c.Shanten;

      Assert.Equal(1, actual);
    }

    [Fact]
    public void BlockingDaiminkan()
    {
      var parser = new ShorthandParser("12223m11222333z");
      var c = new HandCalculator(parser);
      var m2 = TileType.FromSuitAndIndex(Suit.Manzu, 1);
      c.Daiminkan(m2);

      var actual = c.Shanten;

      Assert.Equal(1, actual);
    }

    [Fact]
    public void BlockingShouminkan()
    {
      var parser = new ShorthandParser("1223m112223337z");
      var c = new HandCalculator(parser);
      var m2 = TileType.FromSuitAndIndex(Suit.Manzu, 1);
      var z7 = TileType.FromSuitAndIndex(Suit.Jihai, 6);
      c.Pon(m2);
      c.Discard(z7);
      c.Draw(m2);
      c.Shouminkan(m2);

      var actual = c.Shanten;

      Assert.Equal(1, actual);
    }

    [Fact]
    public void BlockingPon()
    {
      var parser = new ShorthandParser("1223m112223z123M");
      var c = new HandCalculator(parser);
      var m2 = TileType.FromSuitAndIndex(Suit.Manzu, 1);
      c.Pon(m2);
      var shaa = TileType.FromSuitAndIndex(Suit.Jihai, 2);
      c.Discard(shaa);

      var actual = c.Shanten;

      Assert.Equal(1, actual);
    }
  }
}
