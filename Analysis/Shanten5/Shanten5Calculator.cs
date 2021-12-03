using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using Spines.Mahjong.Analysis.Resources;

namespace Spines.Mahjong.Analysis.Shanten5
{
  /// <summary>
  /// Calculates shanten by ignoring melds. For example, this means 13m11222333z 2222M is considered tenpai.
  /// </summary>
  public class Shanten5Calculator
  {
    static Shanten5Calculator()
    {
      LookupSuit = Resource.Vector128Lookup("Shanten5", "suit.dat");
      LookupHonor = Resource.Vector128Lookup("Shanten5", "honor.dat");
    }

    public void Ankan(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] -= 4 * tileType.Base5Value;
      _meldCount += 1;

      UpdateAb(tileType.SuitId);
    }

    public static int Calculate(int[] tileCounts, int meldCount)
    {
      Span<int> base5Hashes = stackalloc int[4];
      var tileTypeId = 0;
      for (var suitId = 0; suitId < 4; suitId++)
      {
        for (var index = 0; index < 9 && tileTypeId < 34; index++)
        {
          base5Hashes[suitId] += Base5.Table[index] * tileCounts[tileTypeId];
          tileTypeId += 1;
        }
      }

      var m = LookupSuit[base5Hashes[0]];
      var p = LookupSuit[base5Hashes[1]];
      var a = CalculatePhase1(m, p);
      var s = LookupSuit[base5Hashes[2]];
      var z = LookupHonor[base5Hashes[3]];
      var b = CalculatePhase1(s, z);
      return CalculateInternal(meldCount, a, b);
    }

    public void Chii(Tile handTile0, Tile handTile1)
    {
      _base5Hashes[handTile0.SuitId] -= handTile0.Base5Value;
      _base5Hashes[handTile1.SuitId] -= handTile1.Base5Value;
      _meldCount += 1;

      UpdateAb(handTile0.SuitId);
    }

    public void Daiminkan(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] -= 3 * tileType.Base5Value;
      _meldCount += 1;

      UpdateAb(tileType.SuitId);
    }

    public void Discard(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] -= tileType.Base5Value;

      UpdateAb(tileType.SuitId);
    }

    public void Draw(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] += tileType.Base5Value;

      UpdateAb(tileType.SuitId);
    }

    public void Haipai(IEnumerable<Tile> tiles)
    {
      foreach (var tile in tiles)
      {
        _base5Hashes[tile.SuitId] += tile.Base5Value;
      }

      var m = LookupSuit[_base5Hashes[0]];
      var p = LookupSuit[_base5Hashes[1]];
      _a = CalculatePhase1(m, p);
      var s = LookupSuit[_base5Hashes[2]];
      var z = LookupHonor[_base5Hashes[3]];
      _b = CalculatePhase1(s, z);
    }

    public void Pon(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] -= 2 * tileType.Base5Value;
      _meldCount += 1;

      UpdateAb(tileType.SuitId);
    }

    public int Shanten()
    {
      return CalculateInternal(_meldCount, _a, _b);
    }

    public void Shouminkan(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] -= tileType.Base5Value;

      UpdateAb(tileType.SuitId);
    }

    private static readonly Vector128<byte>[] LookupSuit;
    private static readonly Vector128<byte>[] LookupHonor;

    private static readonly Vector128<byte>[] ReverseBVectors =
    {
      Vector128.Create(9, 8, 7, 6, 5, 4, 3, 2, 1, 255, 255, 255, 255, 14, 13, 15),
      Vector128.Create(8, 7, 6, 5, 255, 3, 2, 1, 255, 255, 255, 255, 255, 255, 255, 255),
      Vector128.Create(7, 6, 5, 255, 255, 2, 1, 255, 255, 255, 255, 255, 255, 255, 255, 255),
      Vector128.Create(6, 5, 255, 255, 255, 1, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255),
      Vector128.Create(5, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255)
    };

    private static readonly Vector128<byte>[] InversionVectors =
    {
      Vector128.Create(14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 255, 255, 255, 14, 14, 7),
      Vector128.Create(11, 11, 11, 11, 255, 11, 11, 11, 11, 255, 255, 255, 255, 255, 255, 255),
      Vector128.Create(8, 8, 8, 255, 255, 8, 8, 8, 255, 255, 255, 255, 255, 255, 255, 255),
      Vector128.Create(5, 5, 255, 255, 255, 5, 5, 255, 255, 255, 255, 255, 255, 255, 255, 255),
      Vector128.Create(2, 255, 255, 255, 255, 2, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255)
    };

    private readonly int[] _base5Hashes = new int[4];
    private Vector128<byte> _a;
    private Vector128<byte> _b;
    private int _meldCount;

    private void UpdateAb(int suitId)
    {
      if (suitId < 2)
      {
        var m = LookupSuit[_base5Hashes[0]];
        var p = LookupSuit[_base5Hashes[1]];
        _a = CalculatePhase1(m, p);
      }
      else
      {
        var s = LookupSuit[_base5Hashes[2]];
        var z = LookupHonor[_base5Hashes[3]];
        _b = CalculatePhase1(s, z);
      }
    }

    /// <summary>
    /// Calculates shanten
    /// </summary>
    /// <param name="meldCount">How many melds have been made</param>
    /// <param name="a">00,01,02,03,04,10,11,12,13,14,__,__,__,k0,k1,cc</param>
    /// <param name="b">00,01,02,03,04,10,11,12,13,14,__,__,__,k0,k1,cc</param>
    /// <returns></returns>
    private static int CalculateInternal(int meldCount, Vector128<byte> a, Vector128<byte> b)
    {
      var b2 = Ssse3.Shuffle(b, ReverseBVectors[meldCount]);
      var r0 = Sse2.Add(a, b2);
      var r1 = Sse2.Subtract(InversionVectors[meldCount], r0);
      var r3 = Sse2.ShiftRightLogical(r1.AsInt16(), 8);
      var r4 = Sse2.Min(r1, r3.AsByte());
      var r5 = Sse41.MinHorizontal(r4.AsUInt16());
      var r6 = (byte) Sse2.ConvertToInt32(r5.AsInt32());
      return r6 - 1;
    }

    /// <summary>
    /// Combines the vectors into a single one with the same layout:
    /// values for (0,0), (0,1) ... (1,3), (1,4) with (pair,groups)
    /// </summary>
    private static Vector128<byte> CalculatePhase1(Vector128<byte> a, Vector128<byte> b)
    {
      // first calculate all the sums, then merge them down with repeated vertical max
      
      var va1 = Ssse3.Shuffle(a, Vector128.Create(0x01_03_02_01_04_03_02_01UL, 0xFF_0D_01_02_01_01_02_01UL).AsByte());
      var vb1 = Ssse3.Shuffle(b, Vector128.Create(0x01_05_06_07_05_06_07_08UL, 0xFF_0E_02_02_03_05_05_06UL).AsByte());

      var vab1 = Sse2.Add(va1, vb1);

      var va2 = Ssse3.Shuffle(a, Vector128.Create(0xFF_07_06_05_08_07_06_05UL, 0xFF_0E_02_FF_03_05_06_05UL).AsByte());
      var vb2 = Ssse3.Shuffle(b, Vector128.Create(0xFF_01_02_03_01_02_03_04UL, 0xFF_0D_01_FF_01_01_01_02UL).AsByte());

      var vab2 = Sse2.Add(va2, vb2);

      var xab1 = Sse2.Max(vab1, vab2);

      var vab21 = Ssse3.Shuffle(xab1, Vector128.Create(0x07_0D_0B_FF_0A_08_01_00UL, 0xFF_0E_FF_FF_06_04_FF_FFUL).AsByte());
      var vab22 = Ssse3.Shuffle(xab1, Vector128.Create(0xFF_FF_0C_FF_FF_09_03_02UL, 0xFF_FF_FF_FF_FF_05_FF_FFUL).AsByte());

      var xab2 = Sse2.Max(vab21, vab22);

      var vab31 = Ssse3.Shuffle(xab2, Vector128.Create(0x02_03_FF_05_06_07_FF_FFUL, 0xFF_0E_FF_FF_FF_0F_00_0AUL).AsByte());
      var vab32 = Ssse3.Shuffle(xab2, Vector128.Create(0xFF_FF_FF_FF_FF_FF_FF_FFUL, 0xFF_FF_FF_FF_FF_FF_01_0BUL).AsByte());

      // calculates chiitoitsu sum and kokushi without pair sum
      // barely not enough space in above calculation for these
      var b2 = Sse2.And(b, Vector128.Create(0UL, 0xFF_00_FF_00_00_00_00_00UL).AsByte());
      var a2 = Sse2.Add(a, b2);

      return Sse2.Max(Sse2.Max(a2, b), Sse2.Max(vab31, vab32));
    }
  }
}