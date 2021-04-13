using System.Collections.Generic;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.Client
{
  internal static class Kuikae
  {
    public static bool IsValidDiscardForNonKanchanChii(TileType calledTileType, TileType discardTileType)
    {
      return !KuikaeTileTypesByCalledTileTypeId[calledTileType.TileTypeId].Contains(discardTileType);
    }

    static Kuikae()
    {
      KuikaeTileTypesByCalledTileTypeId = new List<List<TileType>>();
      for (var i = 0; i < 34; i++)
      {
        var list = new List<TileType> { TileType.FromTileTypeId(i) };
        if (i < 27 && i % 9 > 3)
        {
          list.Add(TileType.FromTileId(i - 3));
        }
        if (i < 27 && i % 9 < 6)
        {
          list.Add(TileType.FromTileId(i + 3));
        }

        KuikaeTileTypesByCalledTileTypeId.Add(list);
      }
    }

    private static readonly List<List<TileType>> KuikaeTileTypesByCalledTileTypeId;
  }
}
