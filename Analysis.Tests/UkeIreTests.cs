using Spines.Mahjong.Analysis.Shanten;
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
  }
}