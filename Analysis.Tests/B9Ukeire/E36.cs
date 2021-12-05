using System.Linq;

namespace Spines.Mahjong.Analysis.Tests.B9Ukeire
{
  internal class E36
  {
    public E36(byte[] v, ulong[] b)
    {
      _v = v;
      _b = b;
    }

    public int Shanten()
    {
      return _v.Min() - 1;
    }

    public ulong Ukeire()
    {
      var b = _b[0];
      var min = _v[0];
      
      for (var i = 1; i < _v.Length; i++)
      {
        if (min == _v[i])
        {
          b |= _b[i];
        }
        else if (min > _v[i])
        {
          min = _v[i];
          b = _b[i];
        }
      }

      return b;
    }

    private readonly ulong[] _b;

    private readonly byte[] _v;
  }
}