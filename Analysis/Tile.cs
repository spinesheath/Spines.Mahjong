using System.Diagnostics;
using System.Linq;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

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
      Base5Value = Base5.Table[Index];
      Base5Vector = Sse41.Insert(Vector128<int>.Zero, Base5.Table[Index], (byte)SuitId);
    }

    /// <summary>
    /// 0-8 for manzu, pinzu and souzu, 0-6 for jihai
    /// </summary>
    public readonly int Index;

    public readonly bool IsAka;

    public readonly int SuitId;

    public readonly int TileId;

    public readonly TileType TileType;

    /// <summary>
    /// Pow(5, index)
    /// </summary>
    public readonly int Base5Value;

    /// <summary>
    /// Base5Value in position SuitId, rest 0
    /// </summary>
    public readonly Vector128<int> Base5Vector;

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