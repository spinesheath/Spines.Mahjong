using System;
using System.Linq;
using System.Text;

namespace Spines.Mahjong.Analysis.B9Ukeire
{
  public class UkeireCalculator
  {
    public UkeireCalculator(int[] hashes, int meldCount)
    {
      var e0 = B9UkeireLookup.Suit(hashes[0]);
      var e1 = B9UkeireLookup.Suit(hashes[1]);
      var e2 = B9UkeireLookup.Suit(hashes[2]);
      var e3 = B9UkeireLookup.Honor(hashes[3]);

      var d0 = DebugString(e0);
      var d1 = DebugString(e1);
      var d2 = DebugString(e2);
      var d3 = DebugString(e3);

      var em = new E9(ExtractV(e0), ExtractB9(e0));
      var ep = new E9(ExtractV(e1), ExtractB9(e1));
      var es = new E9(ExtractV(e2), ExtractB9(e2));
      var ez = new E9(ExtractV(e3), ExtractB9(e3));

      var a = em.CombineWith(ep);
      var b = es.CombineWith(ez);

      var r = a.CombineWith(b, meldCount);

      Shanten = r.Shanten();
      var ukeire = r.Ukeire();
      Ukeire = ukeire;
      UkeireString = CreateUkeireString(ukeire);
    }

    private static string DebugString(ushort[] row)
    {
      string[] configurations = { "00", "01", "02", "03", "04", "10", "11", "12", "13", "14", "__", "__", "__", "k0", "k1", "cc" };

      var sb = new StringBuilder();

      for (var i = 0; i < row.Length; i++)
      {
        var b9 = Convert.ToString(row[i] >> 4, 2).PadLeft(9, '0');
        var v = (row[i] & 15).ToString().PadLeft(2, ' ');
        sb.AppendLine($"{configurations[i]}: {v}, {b9}");
      }

      return sb.ToString();
    }

    public int Shanten { get; }

    public string UkeireString { get; }

    public ulong Ukeire { get; }


    private static byte[] ExtractV(ushort[] source)
    {
      return source.Select(x => (byte) (x & 15)).ToArray();
    }

    private static ushort[] ExtractB9(ushort[] source)
    {
      return source.Select(x => (ushort) (x >> 4)).ToArray();
    }

    private string CreateUkeireString(ulong b36)
    {
      var b = b36;
      var sb = new StringBuilder();
      var tileTypeId = 0;
      var tileInSuit = false;
      const string suits = "mpsz";
      while (b > 0)
      {
        if ((b & 1) == 1)
        {
          sb.Append(tileTypeId % 9 + 1);
          tileInSuit = true;
        }

        if ((tileTypeId % 9 == 8 || tileTypeId == 33) && tileInSuit)
        {
          sb.Append(suits[tileTypeId / 9]);
          tileInSuit = false;
        }

        b >>= 1;
        tileTypeId += 1;
      }

      if (tileInSuit)
      {
        sb.Append(suits[tileTypeId / 9]);
      }

      return sb.ToString();
    }
  }
}