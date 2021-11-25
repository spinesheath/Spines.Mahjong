﻿using System.Runtime.Intrinsics;
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

    public int Calculate(int[] tileCounts, int meldCount)
    {
      var base5Hashes = new int[4];
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
      var s = LookupSuit[base5Hashes[2]];
      var z = LookupSuit[base5Hashes[3]];
      var a = CalculatePhase1(m, p);
      var b = CalculatePhase1(s, z);

      var sb = Vector128.Create(9, 8, 7, 6, 5, 4, 3, 2, 1, 255, 255, 255, 255, 255, 255, 255);
      var b2 = Ssse3.Shuffle(b, sb);

      // TODO meldCount

      var r = Sse2.Add(a, b2);

      // horizontal max
      var neg = Vector128.Create((byte)14);
      var r1 = Sse2.Subtract(neg, r);
      var r3 = Sse2.ShiftRightLogical(r1.AsInt16(), 8);
      var r4 = Sse2.Min(r1, r3.AsByte());
      var r5 = Sse41.MinHorizontal(r4.AsUInt16());
      var r6 = (byte)Sse2.ConvertToInt32(r5.AsInt32());
      return r6 - 1;
    }

    /// <summary>
    /// Combines the vectors into a single one with the same layout: values for (0,0), (0,1) ... (1,3), (1,4) with (pair,groups)
    /// </summary>
    private static Vector128<byte> CalculatePhase1(Vector128<byte> a, Vector128<byte> b)
    {
      // first calculate all the sums, then merge them down with repeated vertical max

      var sa1 = Vector128.Create(1, 2, 3, 4, 1, 2, 3, 1, 1, 2, 1, 1, 2, 1, 255, 255);
      var va1 = Ssse3.Shuffle(a, sa1);
      var sb1 = Vector128.Create(8, 7, 6, 5, 7, 6, 5, 1, 6, 5, 5, 3, 2, 2, 255, 255);
      var vb1 = Ssse3.Shuffle(b, sb1);

      var vab1 = Sse2.Add(va1, vb1);

      var sa2 = Vector128.Create(5, 6, 7, 8, 5, 6, 7, 255, 5, 6, 5, 3, 255, 2, 255, 255);
      var va2 = Ssse3.Shuffle(a, sa2);
      var sb2 = Vector128.Create(4, 3, 2, 1, 3, 2, 1, 255, 2, 1, 1, 1, 255, 1, 255, 255);
      var vb2 = Ssse3.Shuffle(b, sb2);

      var vab2 = Sse2.Add(va2, vb2);

      var xab1 = Sse2.Max(vab1, vab2);

      var sab11 = Vector128.Create(0, 1, 8, 10, 255, 11, 13, 7, 255, 255, 4, 6, 255, 255, 255, 255);
      var sab12 = Vector128.Create(2, 3, 9, 255, 255, 12, 255, 255, 255, 255, 5, 255, 255, 255, 255, 255);

      var vab21 = Ssse3.Shuffle(xab1, sab11);
      var vab22 = Ssse3.Shuffle(xab1, sab12);

      var xab2 = Sse2.Max(vab21, vab22);

      var sab21 = Vector128.Create(255, 255, 7, 6, 5, 255, 3, 2, 10, 0, 255, 255, 255, 255, 255, 255);
      var sab22 = Vector128.Create(255, 255, 255, 255, 255, 255, 255, 255, 11, 1, 255, 255, 255, 255, 255, 255);

      var vab31 = Ssse3.Shuffle(xab2, sab21);
      var vab32 = Ssse3.Shuffle(xab2, sab22);

      // TODO is max(max(vma vb), xab3) better?
      var xab3 = Sse2.Max(vab31, vab32);
      var xab4 = Sse2.Max(a, xab3);

      return Sse2.Max(b, xab4);
    }

    private static readonly Vector128<byte>[] LookupSuit;
    private static readonly Vector128<byte>[] LookupHonor;
  }
}