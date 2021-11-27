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
      _base5Hashes[tileType.SuitId] -= 4 * Base5.Table[tileType.Index];
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

      return CalculateInternal(meldCount, CalculateA(base5Hashes), CalculateB(base5Hashes));
    }

    public void Chii(Tile handTile0, Tile handTile1)
    {
      _base5Hashes[handTile0.TileType.SuitId] -= Base5.Table[handTile0.TileType.Index];
      _base5Hashes[handTile1.TileType.SuitId] -= Base5.Table[handTile1.TileType.Index];
      _meldCount += 1;

      UpdateAb(handTile0.TileType.SuitId);
    }

    public void Daiminkan(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] -= 3 * Base5.Table[tileType.Index];
      _meldCount += 1;

      UpdateAb(tileType.SuitId);
    }

    public void Discard(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] -= Base5.Table[tileType.Index];

      UpdateAb(tileType.SuitId);
    }

    public void Draw(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] += Base5.Table[tileType.Index];

      UpdateAb(tileType.SuitId);
    }

    public void Haipai(IEnumerable<Tile> tiles)
    {
      foreach (var tile in tiles)
      {
        _base5Hashes[tile.TileType.SuitId] += Base5.Table[tile.TileType.Index];
      }

      _a = CalculateA(_base5Hashes);
      _b = CalculateB(_base5Hashes);
    }

    public void Pon(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] -= 2 * Base5.Table[tileType.Index];
      _meldCount += 1;

      UpdateAb(tileType.SuitId);
    }

    public int Shanten()
    {
      return CalculateInternal(_meldCount, _a, _b);
    }

    public void Shouminkan(TileType tileType)
    {
      _base5Hashes[tileType.SuitId] -= Base5.Table[tileType.Index];

      UpdateAb(tileType.SuitId);
    }

    private static readonly Vector128<byte>[] LookupSuit;
    private static readonly Vector128<byte>[] LookupHonor;
    private static readonly Vector128<byte> KokushiPairSelector = Vector128.Create((byte) 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0);
    private static readonly Vector128<byte> Phase1ShuffleA1 = Vector128.Create((byte) 1, 2, 3, 4, 1, 2, 3, 1, 1, 2, 1, 1, 2, 1, 13, 15);
    private static readonly Vector128<byte> Phase1ShuffleB1 = Vector128.Create((byte) 8, 7, 6, 5, 7, 6, 5, 1, 6, 5, 5, 3, 2, 2, 13, 15);
    private static readonly Vector128<byte> Phase1ShuffleA2 = Vector128.Create(5, 6, 7, 8, 5, 6, 7, 255, 5, 6, 5, 3, 255, 2, 255, 255);
    private static readonly Vector128<byte> Phase1ShuffleB2 = Vector128.Create(4, 3, 2, 1, 3, 2, 1, 255, 2, 1, 1, 1, 255, 1, 255, 255);
    private static readonly Vector128<byte> Phase1ShuffleAb11 = Vector128.Create(0, 1, 8, 10, 255, 11, 13, 7, 255, 255, 4, 6, 255, 255, 14, 15);
    private static readonly Vector128<byte> Phase1ShuffleAb12 = Vector128.Create(2, 3, 9, 255, 255, 12, 255, 255, 255, 255, 5, 255, 255, 255, 255, 255);
    private static readonly Vector128<byte> Phase1ShuffleAb21 = Vector128.Create(255, 255, 7, 6, 5, 255, 3, 2, 10, 0, 15, 255, 255, 14, 255, 255);
    private static readonly Vector128<byte> Phase1ShuffleAb22 = Vector128.Create(255, 255, 255, 255, 255, 255, 255, 255, 11, 1, 255, 255, 255, 255, 255, 255);

    private static readonly Vector128<byte>[] MeldCountVectors =
    {
      Vector128.Create(9, 8, 7, 6, 5, 4, 3, 2, 1, 255, 10, 255, 255, 13, 255, 14),
      Vector128.Create(8, 7, 6, 5, 255, 3, 2, 1, 255, 255, 255, 255, 255, 255, 255, 255),
      Vector128.Create(7, 6, 5, 255, 255, 2, 1, 255, 255, 255, 255, 255, 255, 255, 255, 255),
      Vector128.Create(6, 5, 255, 255, 255, 1, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255),
      Vector128.Create(5, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255)
    };

    private static readonly Vector128<byte>[] ExcessGroupClearingVectors =
    {
      Vector128.Create((byte) 255),
      Vector128.Create(255, 255, 255, 255, 0, 255, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0),
      Vector128.Create(255, 255, 255, 0, 0, 255, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0),
      Vector128.Create(255, 255, 0, 0, 0, 255, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0),
      Vector128.Create(255, 0, 0, 0, 0, 255, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0)
    };

    private static readonly Vector128<byte>[] InversionVectors =
    {
      Vector128.Create(14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 7, 255, 255, 14, 254, 254),
      Vector128.Create(14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 255, 255, 255, 255, 254, 255),
      Vector128.Create(14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 255, 255, 255, 255, 254, 255),
      Vector128.Create(14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 255, 255, 255, 255, 254, 255),
      Vector128.Create(14, 14, 14, 14, 14, 14, 14, 14, 14, 14, 255, 255, 255, 255, 255, 255)
    };

    private readonly int[] _base5Hashes = new int[4];
    private Vector128<byte> _a;
    private Vector128<byte> _b;
    private int _meldCount;

    private void UpdateAb(int suitId)
    {
      if (suitId < 2)
      {
        _a = CalculateA(_base5Hashes);
      }
      else
      {
        _b = CalculateB(_base5Hashes);
      }
    }

    private static int CalculateInternal(int meldCount, Vector128<byte> a, Vector128<byte> b)
    {
      var b2 = Ssse3.Shuffle(b, MeldCountVectors[meldCount]);
      var r = Sse2.And(Sse2.Add(a, b2), ExcessGroupClearingVectors[meldCount]);

      var r1 = Sse2.Subtract(InversionVectors[meldCount], r);
      var r3 = Sse2.ShiftRightLogical(r1.AsInt16(), 8);
      var r4 = Sse2.Min(r1, r3.AsByte());
      var k2 = Sse2.And(r4, KokushiPairSelector);
      var k3 = Sse2.ShiftRightLogical(k2.AsUInt32(), 16);
      var k4 = k3.AsByte();
      var r5 = Sse2.Subtract(r4, k4);
      var r6 = Sse41.MinHorizontal(r5.AsUInt16());
      var r7 = (byte) Sse2.ConvertToInt32(r6.AsInt32());
      return r7 - 1 - 3 * meldCount;
    }

    private static Vector128<byte> CalculateB(Span<int> base5Hashes)
    {
      var s = LookupSuit[base5Hashes[2]];
      var z = LookupHonor[base5Hashes[3]];
      var b = CalculatePhase1(s, z);
      return b;
    }

    private static Vector128<byte> CalculateA(Span<int> base5Hashes)
    {
      var m = LookupSuit[base5Hashes[0]];
      var p = LookupSuit[base5Hashes[1]];
      var a = CalculatePhase1(m, p);
      //// TODO chiitoi is adding up with kokushi pair, fix that in lookup table and this insert can be removed
      return Sse41.Insert(a, 0, 15);
    }

    /// <summary>
    /// Combines the vectors into a single one with the same layout: values for (0,0), (0,1) ... (1,3), (1,4) with
    /// (pair,groups)
    /// </summary>
    private static Vector128<byte> CalculatePhase1(Vector128<byte> a, Vector128<byte> b)
    {
      // first calculate all the sums, then merge them down with repeated vertical max

      var va1 = Ssse3.Shuffle(a, Phase1ShuffleA1);
      var vb1 = Ssse3.Shuffle(b, Phase1ShuffleB1);

      var vab1 = Sse2.Add(va1, vb1);

      var va2 = Ssse3.Shuffle(a, Phase1ShuffleA2);
      var vb2 = Ssse3.Shuffle(b, Phase1ShuffleB2);

      var vab2 = Sse2.Add(va2, vb2);

      var xab1 = Sse2.Max(vab1, vab2);

      var vab21 = Ssse3.Shuffle(xab1, Phase1ShuffleAb11);
      var vab22 = Ssse3.Shuffle(xab1, Phase1ShuffleAb12);

      var xab2 = Sse2.Max(vab21, vab22);

      var vab31 = Ssse3.Shuffle(xab2, Phase1ShuffleAb21);
      var vab32 = Ssse3.Shuffle(xab2, Phase1ShuffleAb22);

      return Sse2.Max(Sse2.Max(a, b), Sse2.Max(vab31, vab32));
    }
  }
}