using System;
using Spines.Mahjong.Analysis.Resources;

namespace Spines.Mahjong.Analysis.B9Ukeire
{
  public static class B9UkeireLookup
  {
    static B9UkeireLookup()
    {
      LookupSuit = Resource.UInt16Lookup("B9Ukeire", "suit.dat");
      LookupHonor = Resource.UInt16Lookup("B9Ukeire", "honor.dat");
    }

    public static ushort[] Honor(int base5Hash)
    {
      var r = new ushort[16];
      var start = base5Hash * 16;
      Array.Copy(LookupHonor, start, r, 0, r.Length);
      return r;
    }

    public static ushort[] Suit(int base5Hash)
    {
      var r = new ushort[16];
      var start = base5Hash * 16;
      Array.Copy(LookupSuit, start, r, 0, r.Length);
      return r;
    }

    private static readonly ushort[] LookupSuit;

    private static readonly ushort[] LookupHonor;
  }
}