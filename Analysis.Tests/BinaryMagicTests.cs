using System;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class BinaryMagicTests
  {
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 0)]
    [InlineData(5, 0)]
    [InlineData(6, 0)]
    [InlineData(7, 0)]
    [InlineData(8, 1)]
    [InlineData(9, 1)]
    [InlineData(10, 0)]
    [InlineData(11, 0)]
    [InlineData(12, 0)]
    [InlineData(13, 0)]
    [InlineData(14, 0)]
    [InlineData(15, 0)]
    [InlineData(16, 0)]
    [InlineData(17, 1)]
    [InlineData(18, 1)]
    [InlineData(19, 0)]
    [InlineData(20, 0)]
    [InlineData(21, 0)]
    [InlineData(22, 0)]
    [InlineData(23, 0)]
    [InlineData(24, 0)]
    [InlineData(25, 0)]
    [InlineData(26, 1)]
    [InlineData(27, 1)]
    [InlineData(28, 1)]
    [InlineData(29, 1)]
    [InlineData(30, 1)]
    [InlineData(31, 1)]
    [InlineData(32, 1)]
    [InlineData(33, 1)]
    public void Kokushi(int x, int expected)
    {
      var r = (((1 << x) & 0b100000001100000001100000001) >> x) | ((x + 5) >> 5);
      Assert.Equal(expected, r);
    }

    [Theory]
    [InlineData(0, 0, 0b1, 1, 0)]
    [InlineData(0, 0, 0b10, 1, 0)]
    [InlineData(0, 1, 0b10, 1, 1)]
    [InlineData(0, 1, 0b100, 0, 1)]
    [InlineData(0, 2, 0b10, 0, 0)]
    [InlineData(0, 3, 0b10, 0, 0)]
    [InlineData(1, 0, 0b1, 0, 0)]
    [InlineData(1, 1, 0b10, 0, 0)]
    [InlineData(1, 1, 0b100, 0, 0)]
    [InlineData(1, 0, 0b10, 0, 0)]
    [InlineData(1, 2, 0b10, 0, 0)]
    [InlineData(1, 3, 0b10, 0, 0)]
    public void KokushiDiscard(int tileTypeId, int tileCountAfterDiscard, int pairs, int expectedShantenChange, int expectedPairShift)
    {
      // (1 << x & 0b100000001100000001100000001) >> x | (x + 5) >> 5
      // 1 if the tileType is a terminal or honor, else 0
      var r = (((1 << tileTypeId) & 0b100000001100000001100000001) >> tileTypeId) | ((tileTypeId + 5) >> 5);

      // 1 if tileCountAfterDiscard < 2, else 0
      var s = ((tileCountAfterDiscard ^ 2) >> 1) & r;
      // 1 if tileCountAfterDiscard == 1, else 0
      var p = tileCountAfterDiscard & s;
      pairs >>= p;
      // 1 if no pair was removed or there were at least two pairs before, else 0
      var t = (pairs | ~p) & s;

      Assert.Equal(expectedShantenChange, t);
      Assert.Equal(expectedPairShift, p);
    }

    [Theory]
    [InlineData(1, 0, 0, 1, 0)]
    [InlineData(1, 0, 1, 1, 0)]
    [InlineData(1, 1, 1, 1, 1)]
    [InlineData(1, 1, 2, 0, 1)]
    [InlineData(1, 2, 1, 0, 0)]
    [InlineData(1, 3, 1, 0, 0)]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(0, 1, 1, 0, 0)]
    [InlineData(0, 1, 2, 0, 0)]
    [InlineData(0, 0, 1, 0, 0)]
    [InlineData(0, 2, 1, 0, 0)]
    [InlineData(0, 3, 1, 0, 0)]
    public void KokushiDiscard2(int kyuuhaiValue, int tileCountAfterDiscard, int pairs, int expectedShantenChange, int expectedPairChange)
    {
      var p = (2 >> tileCountAfterDiscard) & kyuuhaiValue;
      var s = (3 >> (pairs * tileCountAfterDiscard)) & kyuuhaiValue;

      Assert.Equal(expectedShantenChange, s);
      Assert.Equal(expectedPairChange, p);
    }

    [Theory]
    [InlineData(0, 0, 0b1, 1, 0)]
    [InlineData(0, 0, 0b10, 1, 0)]
    [InlineData(0, 1, 0b1, 1, 1)]
    [InlineData(0, 1, 0b10, 0, 1)]
    [InlineData(0, 2, 0b1, 0, 0)]
    [InlineData(0, 3, 0b1, 0, 0)]
    [InlineData(1, 0, 0b1, 0, 0)]
    [InlineData(1, 1, 0b1, 0, 0)]
    [InlineData(1, 1, 0b10, 0, 0)]
    [InlineData(1, 0, 0b10, 0, 0)]
    [InlineData(1, 2, 0b1, 0, 0)]
    [InlineData(1, 3, 0b1, 0, 0)]
    public void KokushiDraw(int tileTypeId, int previousTileCount, int pairs, int expectedShantenChange, int expectedPairShift)
    {
      // (1 << x & 0b100000001100000001100000001) >> x | (x + 5) >> 5
      // 1 if the tileType is a terminal or honor, else 0
      var r = (((1 << tileTypeId) & 0b100000001100000001100000001) >> tileTypeId) | ((tileTypeId + 5) >> 5);

      // 1 if previous < 2, else 0
      var s = ((previousTileCount ^ 2) >> 1) & r;
      // 1 if previous == 1, else 0
      var p = previousTileCount & s;
      // 1 if no pair was added or there were no pairs before, else 0
      var t = (pairs | ~p) & s;

      Assert.Equal(expectedShantenChange, t);
      Assert.Equal(expectedPairShift, p);
    }

    [Theory]
    [InlineData(1, 0, 0, 1, 0)]
    [InlineData(1, 0, 1, 1, 0)]
    [InlineData(1, 1, 0, 1, 1)]
    [InlineData(1, 1, 1, 0, 1)]
    [InlineData(1, 2, 0, 0, 0)]
    [InlineData(1, 3, 0, 0, 0)]
    [InlineData(0, 0, 0, 0, 0)]
    [InlineData(0, 1, 0, 0, 0)]
    [InlineData(0, 1, 1, 0, 0)]
    [InlineData(0, 0, 1, 0, 0)]
    public void KokushiDraw2(int kyuuhaiValue, int previousTileCount, int pairs, int expectedShantenChange, int expectedPairChange)
    {
      var p = (2 >> previousTileCount) & kyuuhaiValue;
      var s = kyuuhaiValue >> (pairs * previousTileCount);

      Assert.Equal(expectedShantenChange, s);
      Assert.Equal(expectedPairChange, p);
    }

    [Theory]
    [InlineData(-1, 1, 8)]
    [InlineData(-1, 8)]
    [InlineData(-1, 0, 1, 2, 4, 5, 6, 7, 8)]
    [InlineData(0)]
    [InlineData(0, 4)]
    [InlineData(0, 0, 8)]
    [InlineData(0, 0, 1, 2, 3, 4, 5, 6, 7, 8)]
    [InlineData(0, 0, 2, 3, 4, 5, 6, 8)]
    [InlineData(1, 0)]
    [InlineData(1, 0, 7)]
    [InlineData(1, 0, 1, 2, 3, 4, 5, 7, 8)]
    [InlineData(1, 0, 4, 5, 6, 7)]
    public void Weights(int expectedSign, params int[] tileTypeIndexes)
    {
      // +1 represents a draw
      const int actionValue = 1;
      var w = 0;
      foreach (var tileTypeIndex in tileTypeIndexes)
      {
        // negative/positive around the center
        var t = 4 - tileTypeIndex;
        // +1 or -1
        var sign = 1 | (t >> 31);
        // shift by 4 bits for each step away from the center
        var s = ((sign * t) << 2) - 4;
        // left of center adds, right of center subtracts. Mask eliminates center.
        w += sign * ((actionValue << s) & 0b_11111111_11111111);
      }

      Assert.Equal(Math.Sign(expectedSign), Math.Sign(w));
    }
  }
}