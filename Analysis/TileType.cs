using System.Diagnostics;

namespace Spines.Mahjong.Analysis
{
  public class TileType
  {
    private TileType(int tileTypeId)
    {
      TileTypeId = tileTypeId;
      Index = tileTypeId % 9;
      SuitId = tileTypeId / 9;
      Suit = (Suit) SuitId;
    }

    public int Index { get; }

    public Suit Suit { get; }

    public int SuitId { get; }

    public int TileTypeId { get; }

    /// <summary>
    /// 0-135
    /// </summary>
    public static TileType FromTileId(int tileId)
    {
      return FromTileTypeId(tileId / 4);
    }

    /// <summary>
    /// 0-34
    /// </summary>
    public static TileType FromTileTypeId(int tileTypeId)
    {
      Debug.Assert(tileTypeId >= 0 && tileTypeId < 34);
      return ByTileType[tileTypeId];
    }

    /// <summary>
    /// 0-3 and 0-9
    /// </summary>
    public static TileType FromSuitAndIndex(Suit suit, int index) => FromTileTypeId((int) suit * 9 + index);

    private static readonly TileType[] ByTileType =
    {
      new TileType(0),
      new TileType(1),
      new TileType(2),
      new TileType(3),
      new TileType(4),
      new TileType(5),
      new TileType(6),
      new TileType(7),
      new TileType(8),
      new TileType(9),
      new TileType(10),
      new TileType(11),
      new TileType(12),
      new TileType(13),
      new TileType(14),
      new TileType(15),
      new TileType(16),
      new TileType(17),
      new TileType(18),
      new TileType(19),
      new TileType(20),
      new TileType(21),
      new TileType(22),
      new TileType(23),
      new TileType(24),
      new TileType(25),
      new TileType(26),
      new TileType(27),
      new TileType(28),
      new TileType(29),
      new TileType(30),
      new TileType(31),
      new TileType(32),
      new TileType(33)
    };
  }
}