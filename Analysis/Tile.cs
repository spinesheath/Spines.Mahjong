using System.Diagnostics;

namespace Spines.Mahjong.Analysis
{
  public class Tile
  {
    private Tile(int tileTypeId)
    {
      TileTypeId = tileTypeId;
      Index = tileTypeId % 9;
      Suit = (Suit) (tileTypeId / 9);
    }

    public int Index { get; }

    public Suit Suit { get; }

    public int TileTypeId { get; }

    public static Tile FromTileId(int tileId)
    {
      return FromTileTypeId(tileId / 4);
    }

    public static Tile FromTileTypeId(int tileTypeId)
    {
      Debug.Assert(tileTypeId >= 0 && tileTypeId < 34);
      return ByTileType[tileTypeId];
    }

    public static Tile FromSuitAndIndex(Suit suit, int index) => FromTileTypeId((int) suit * 9 + index);

    private static readonly Tile[] ByTileType =
    {
      new Tile(0),
      new Tile(1),
      new Tile(2),
      new Tile(3),
      new Tile(4),
      new Tile(5),
      new Tile(6),
      new Tile(7),
      new Tile(8),
      new Tile(9),
      new Tile(10),
      new Tile(11),
      new Tile(12),
      new Tile(13),
      new Tile(14),
      new Tile(15),
      new Tile(16),
      new Tile(17),
      new Tile(18),
      new Tile(19),
      new Tile(20),
      new Tile(21),
      new Tile(22),
      new Tile(23),
      new Tile(24),
      new Tile(25),
      new Tile(26),
      new Tile(27),
      new Tile(28),
      new Tile(29),
      new Tile(30),
      new Tile(31),
      new Tile(32),
      new Tile(33)
    };
  }
}