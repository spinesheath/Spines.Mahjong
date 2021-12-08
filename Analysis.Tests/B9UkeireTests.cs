using System.Linq;
using System.Text;
using Spines.Mahjong.Analysis.B9Ukeire;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class B9UkeireTests
  {
    [Theory]
    [InlineData("123456789m123p11s", -1, "")]
    [InlineData("123456789m13p11s", 0, "2p")]
    [InlineData("468m1156778p345s", 1, "57m69p")]
    [InlineData("123459m34079p45s", 2, "1234569m789p3456s")]
    public void Experiments(string hand, int expectedShanten, string expectedUkeire)
    {
      var parser = new ShorthandParser(hand);
      var hashes = new int[4];
      foreach (var tile in parser.Tiles)
      {
        hashes[tile.SuitId] += Base5.Table[tile.Index];
      }

      var e0 = B9UkeireLookup.Suit(hashes[0]);
      var e1 = B9UkeireLookup.Suit(hashes[1]);
      var e2 = B9UkeireLookup.Suit(hashes[2]);
      var e3 = B9UkeireLookup.Honor(hashes[3]);
      
      var em = new E9(ExtractV(e0), ExtractB9(e0));
      var ep = new E9(ExtractV(e1), ExtractB9(e1));
      var es = new E9(ExtractV(e2), ExtractB9(e2));
      var ez = new E9(ExtractV(e3), ExtractB9(e3));

      var a = em.CombineWith(ep);
      var b = es.CombineWith(ez);

      var r = a.CombineWith(b, parser.Melds.Count());

      var shanten = r.Shanten();
      var ukeire = UkeireString(r.Ukeire());

      Assert.Equal(expectedShanten, shanten);
      Assert.Equal(expectedUkeire, ukeire);
    }

    private byte[] ExtractV(ushort[] source)
    {
      return source.Select(x => (byte)(x & 15)).ToArray();
    }

    private ushort[] ExtractB9(ushort[] source)
    {
      return source.Select(x => (ushort)(x >> 4)).ToArray();
    }

    private string UkeireString(ulong b36)
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
