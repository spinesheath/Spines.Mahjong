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
    public int Index { get; }

    public bool IsAka { get; }

    public int SuitId { get; }

    public int TileId { get; }

    public TileType TileType { get; }

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