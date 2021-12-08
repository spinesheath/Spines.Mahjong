namespace Spines.Mahjong.Analysis.B9Ukeire
{
  public class E18
  {
    public E18(byte[] v, uint[] b)
    {
      _v = v;
      _b = b;
    }

    private readonly byte[] _v;

    private readonly uint[] _b;

    public E36 CombineWith(E18 other, int meldCount)
    {
      var v = new byte[13];
      var b = new ulong[13];

      // meldCount = 0: 00+14, 01+13, 02+12, 03+11, 04+10, 10+04, 11+03, 12+02, 13+01, 14+00
      // meldCount = 1: 00+13, 01+12, 02+11, 03+10,        10+03, 11+02, 12+01, 13+00
      // meldCount = 2: 00+12, 01+11, 02+10,               10+02, 11+01, 12+00
      // meldCount = 3: 00+11, 01+10,                      10+01, 11+00
      // meldCount = 4: 00+10,                             10+00
      for (var i = 0; i < 5 - meldCount; i++)
      {
        Combine(v, b, i, other, i, 9 - i - meldCount);
        Combine(v, b, i + 5, other, 9 - i - meldCount, i);
      }

      if (meldCount == 0)
      {
        // cc+cc
        Combine(v, b, 10, other, 10, 10);

        // k0+k1
        Combine(v, b, 11, other, 11, 12);

        // k1+k0
        Combine(v, b, 12, other, 12, 11);
      }

      // invert
      var r = new byte[13];
      for (var i = 0; i < r.Length; i++)
      {
        r[i] = (byte)(14 - 3 * meldCount - v[i]);
      }

      if (meldCount == 0)
      {
        r[10] -= 7;
      }

      return new E36(r, b);
    }

    private void Combine(byte[] v, ulong[] b, int targetIndex, E18 other, int sourceIndexA, int sourceIndexB)
    {
      var combinedValue = (byte)(_v[sourceIndexA] + other._v[sourceIndexB]);
      var b18 = ((ulong)other._b[sourceIndexB] << 18) | _b[sourceIndexA];

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
  }
}