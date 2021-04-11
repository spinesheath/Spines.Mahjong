﻿using Spines.Mahjong.Analysis.Shanten;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class UkeIreTests
  {
    [Theory]
    [InlineData("1112345678999m", new[] {1, 3, 3, 3, 3, 3, 3, 3, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1})]
    [InlineData("1m111M222M333M444M", new[] { -1, -1, -1, -1, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 })]
    public void JustSomeHands(string hand, int[] expected)
    {
      var parser = new ShorthandParser(hand);
      var c = new HandCalculator(parser);

      var actual = c.GetUkeIreFor13();

      Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("123456789m12345s", "1s")]
    [InlineData("468m11156778p345s", "4m")]
    [InlineData("689m11156778p345s", "6m")]
    [InlineData("113467m345p66s111z", "4m")]
    [InlineData("34589m235p124s344z", "5p")]
    public void HighestUkeIreDiscard(string hand, string expectedTileType)
    {
      var expected = TileType.FromString(expectedTileType).TileTypeId;

      var parser = new ShorthandParser(hand);
      var c = new UkeIreCalculator(parser);

      var actual = c.GetHighestUkeIreDiscard();

      Assert.Equal(expected, actual);
    }
  }
}