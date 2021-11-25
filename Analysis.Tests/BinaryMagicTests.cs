using System;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
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

    [Fact]
    public void Shanten5Experiments()
    {
      // 123456789m147p1s  2-shanten
      // (0,0), (0,1), (0,2), (0,3), (0,4), (1,0), (1,1), (1,2), (1,3), (1,4)
      var m = new byte[] { 0, 3, 6, 9, 9, 1, 4, 7, 9, 9 };
      var p = new byte[] { 0, 1, 2, 3, 3, 1, 2, 3, 3, 3 };
      var s = new byte[] { 0, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
      var z = new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

      var vm = Vector128.Create(0, m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], 0, 0, 0, 0, 0, 0);
      var vp = Vector128.Create(0, p[1], p[2], p[3], p[4], p[5], p[6], p[7], p[8], p[9], 0, 0, 0, 0, 0, 0);

      var sm1 = Vector128.Create(1, 2, 3, 4, 1, 2, 3, 1, 1, 2, 1, 1, 2, 1, 255, 255);
      var vm1 = Ssse3.Shuffle(vm, sm1);
      var sp1 = Vector128.Create(8, 7, 6, 5, 7, 6, 5, 1, 6, 5, 5, 3, 2, 2, 255, 255);
      var vp1 = Ssse3.Shuffle(vp, sp1);

      var vmp1 = Sse2.Add(vm1, vp1);

      var sm2 = Vector128.Create(5, 6, 7, 8, 5, 6, 7, 255, 5, 6, 5, 3, 255, 2, 255, 255);
      var vm2 = Ssse3.Shuffle(vm, sm2);
      var sp2 = Vector128.Create(4, 3, 2, 1, 3, 2, 1, 255, 2, 1, 1, 1, 255, 1, 255, 255);
      var vp2 = Ssse3.Shuffle(vp, sp2);

      var vmp2 = Sse2.Add(vm2, vp2);
      
      var xmp1 = Sse2.Max(vmp1, vmp2);

      var smp11 = Vector128.Create(0, 1, 8, 10, 255, 11, 13, 7, 255, 255, 4, 6, 255, 255, 255, 255);
      var smp12 = Vector128.Create(2, 3, 9, 255, 255, 12, 255, 255, 255, 255, 5, 255, 255, 255, 255, 255);

      var vmp21 = Ssse3.Shuffle(xmp1, smp11);
      var vmp22 = Ssse3.Shuffle(xmp1, smp12);

      var xmp2 = Sse2.Max(vmp21, vmp22);

      var smp21 = Vector128.Create(255, 255, 7, 6, 5, 255, 3, 2, 10, 0, 255, 255, 255, 255, 255, 255);
      var smp22 = Vector128.Create(255, 255, 255, 255, 255, 255, 255, 255, 11, 1, 255, 255, 255, 255, 255, 255);

      var vmp31 = Ssse3.Shuffle(xmp2, smp21);
      var vmp32 = Ssse3.Shuffle(xmp2, smp22);

      var xmp3 = Sse2.Max(vmp31, vmp32);

      // TODO is max(max(vm, vp), max_mp3) better?
      var xmp4 = Sse2.Max(vm, xmp3);

      var r = Sse2.Max(vp, xmp4); // Here we have the values for (0,0), (0,1) ... (1,3), (1,4) for m+p
    }
  }
}