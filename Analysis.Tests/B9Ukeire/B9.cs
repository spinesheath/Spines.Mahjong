using System.Text;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests.B9Ukeire
{
  public class B9
  {

    [Fact]
    public void Experiments()
    {
      // 2m 111Z 222Z 333Z 444Z
      var meldCount = 4;

      var em = new E9(
        new byte[] {0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0}, 
        new ushort[]
        {
          0b000000000,
          0b000001111,
          0b000001111,
          0b000001111,
          0b000001111,
          0b000000010,
          0b000001111,
          0b000001111,
          0b000001111,
          0b000001111,
          0b000000000,
          0b000000000,
          0b000000000
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
