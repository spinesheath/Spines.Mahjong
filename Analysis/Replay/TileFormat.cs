using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spines.Mahjong.Analysis.Replay
{
  internal static class TileFormat
  {
    public static string ToString(int tileId)
    {
      var suit = "mpsz"[tileId / 4 / 9];
      return $"{tileId / 4 % 9 + 1}{suit}";
    }

    public static string ToString(List<int> tileIds)
    {
      var groups = tileIds.GroupBy(t => t / 4 / 9).OrderBy(g => g.Key);
      var sb = new StringBuilder();
      foreach (var g in groups)
      {
        var suit = "mpsz"[g.Key];
        sb.Append(string.Join("", g.Select(t => t / 4 % 9 + 1)));
        sb.Append(suit);
      }

      return sb.ToString();
    }
  }
}