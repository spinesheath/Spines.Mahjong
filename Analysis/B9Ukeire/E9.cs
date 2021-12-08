namespace Spines.Mahjong.Analysis.B9Ukeire
{
  public class E9
  {
    public E9(byte[] v, ushort[] b)
    {
      _v = v;
      _b = b;
    }

    public E18 CombineWith(E9 other)
    {
      var v = new byte[13];
      var b = new uint[13];

      // 00 (is always nothing)
      Combine(v, b, 0, other, 0, 0);

      // 01: 00+01, 01+00
      Combine(v, b, 1, other, 0, 1);
      Combine(v, b, 1, other, 1, 0);

      // 02: 00+02, 01+01, 02+00
      Combine(v, b, 2, other, 0, 2);
      Combine(v, b, 2, other, 1, 1);
      Combine(v, b, 2, other, 2, 0);

      // 03: 00+03, 01+02, 02+01, 03+00
      Combine(v, b, 3, other, 0, 3);
      Combine(v, b, 3, other, 1, 2);
      Combine(v, b, 3, other, 2, 1);
      Combine(v, b, 3, other, 3, 0);

      // 04: 00+04, 01+03, 02+02, 03+01, 04+00
      Combine(v, b, 4, other, 0, 4);
      Combine(v, b, 4, other, 1, 3);
      Combine(v, b, 4, other, 2, 2);
      Combine(v, b, 4, other, 3, 1);
      Combine(v, b, 4, other, 4, 0);

      // 10: 00+10, 10+00
      Combine(v, b, 5, other, 0, 5);
      Combine(v, b, 5, other, 5, 0);

      // 11: 00+11, 01+10, 10+01, 11+00
      Combine(v, b, 6, other, 0, 6);
      Combine(v, b, 6, other, 1, 5);
      Combine(v, b, 6, other, 5, 1);
      Combine(v, b, 6, other, 6, 0);

      // 12: 00+12, 01+11, 02+10, 10+02, 11+01, 12+00
      Combine(v, b, 7, other, 0, 7);
      Combine(v, b, 7, other, 1, 6);
      Combine(v, b, 7, other, 2, 5);
      Combine(v, b, 7, other, 5, 2);
      Combine(v, b, 7, other, 6, 1);
      Combine(v, b, 7, other, 7, 0);

      // 13: 00+13, 01+12, 02+11, 03+10, 10+03, 11+02, 12+01, 13+00
      Combine(v, b, 8, other, 0, 8);
      Combine(v, b, 8, other, 1, 7);
      Combine(v, b, 8, other, 2, 6);
      Combine(v, b, 8, other, 3, 5);
      Combine(v, b, 8, other, 5, 3);
      Combine(v, b, 8, other, 6, 2);
      Combine(v, b, 8, other, 7, 1);
      Combine(v, b, 8, other, 8, 0);

      // 14: 00+14, 01+13, 02+12, 03+11, 04+10, 10+04, 11+03, 12+02, 13+01, 14+00
      Combine(v, b, 9, other, 0, 8);
      Combine(v, b, 9, other, 1, 7);
      Combine(v, b, 9, other, 2, 6);
      Combine(v, b, 9, other, 3, 5);
      Combine(v, b, 9, other, 5, 3);
      Combine(v, b, 9, other, 6, 2);
      Combine(v, b, 9, other, 7, 1);
      Combine(v, b, 9, other, 8, 0);

      // cc
      Combine(v, b, 10, other, 10, 10);

      // k0
      Combine(v, b, 11, other, 11, 11);

      // k1
      Combine(v, b, 12, other, 11, 12);
      Combine(v, b, 12, other, 12, 11);

      return new E18(v, b);
    }

    private void Combine(byte[] v, uint[] b, int targetIndex, E9 other, int sourceIndexA, int sourceIndexB)
    {
      var combinedValue = (byte)(_v[sourceIndexA] + other._v[sourceIndexB]);
      var b18 = ((uint) other._b[sourceIndexB] << 9) | _b[sourceIndexA];

      if (v[targetIndex] < combinedValue)
      {
        v[targetIndex] = combinedValue;
        b[targetIndex] = b18;
      }
      else if (v[targetIndex] == combinedValue)
      {
        b[targetIndex] |= b18;
      }
    }

    private readonly ushort[] _b;

    private readonly byte[] _v;
  }
}