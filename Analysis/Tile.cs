using System.Diagnostics;
using System.Linq;

namespace Spines.Mahjong.Analysis
{
  public class Tile
  {
    static Tile()
    {
      ByTileId = Enumerable.Range(0, 136).Select(i => new Tile(i)).ToArray();
    }

    private Tile(int tileId)
    {
      TileId = tileId;
      TileType = TileType.FromTileId(tileId);
      IsAka = tileId == 16 || tileId == 52 || tileId == 88;
      SuitId = TileType.SuitId;
      Index = TileType.Index;
    }

    /// <summary>
    /// 0-8 for manzu, pinzu and souzu, 0-6 for jihai
    /// </summary>
    public readonly int Index;

    public readonly bool IsAka;

    public readonly int SuitId;

    public readonly int TileId;

    public readonly TileType TileType;

    public static Tile FromTileId(int tileId)
    {
      return ByTileId[tileId];
    }

    public static Tile FromTileType(TileType tileType, int index)
    {
      Debug.Assert(index >= 0 && index < 4, "4 tiles per tileType");
      return ByTileId[tileType.TileTypeId * 4 + index];
    }

    public override string ToString()
    {
      return IsAka ? $"0{"mps"[SuitId]}" : TileType.ToString();
    }

    private static readonly Tile[] ByTileId;
  }
}