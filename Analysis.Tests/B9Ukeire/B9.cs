using System.Text;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests.B9Ukeire
{
  public class B9
  {

    [Fact]
    public void Experiments()
    {
      // 2222m 222Z 333Z 444Z
      var meldCount = 3;

      var em = new E9(
        new byte[] {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0}, 
        new ushort[]
        {
          0b000000000, // 00
          0b000000000, // 01
          0b000001101, // 02
          0b111111101, // 03
          0b111111101, // 04
          0b000000000, // 10
          0b111111101, // 11
          0b111111101, // 12
          0b111111101, // 13
          0b111111101, // 14
          0b000000000, // cc
          0b100000001, // k0
          0b100000001  // k1
        });
      
      var ep = new E9(new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, new ushort[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0});
      var es = new E9(new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, new ushort[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0});
      var ez = new E9(new byte[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}, new ushort[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0});

      var a = em.CombineWith(ep);
      var b = es.CombineWith(ez);

      var r = a.CombineWith(b, meldCount);

      var shanten = r.Shanten();
      var ukeire = UkeireString(r.Ukeire());
    }

    private string UkeireString(ulong b)
    {
      var sb = new StringBuilder();
      var tileTypeId = 0;
      while (b > 0)
      {
        if ((b & 1) == 1)
        {
          sb.Append(TileType.FromTileTypeId(tileTypeId));
        }

        b >>= 1;
        tileTypeId += 1;
      }

      return sb.ToString();
    }
  }
}
