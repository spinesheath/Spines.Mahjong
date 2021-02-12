namespace Spines.Mahjong.Analysis.Shanten
{
  public struct Tile
  {
    public Suit Suit { get; set; }

    public int Index { get; set; }

    internal int TileType => (int) Suit * 9 + Index;

    internal static Tile FromTileType(int tileType)
    {
      return new Tile {Index = tileType % 9, Suit = (Suit) (tileType / 9)};
    }
  }
}