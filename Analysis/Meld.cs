using System.Collections.Generic;

namespace Spines.Mahjong.Analysis
{
  public struct Meld
  {
    public Suit Suit { get; set; }

    public int MeldId { get; set; }

    public IEnumerable<Tile> Tiles => GetTiles();

    internal Meld(Suit suit, int meldId)
    {
      MeldId = meldId;
      Suit = suit;
    }

    private IEnumerable<Tile> GetTiles()
    {
      if (MeldId < 7)
      {
        yield return Tile.FromSuitAndIndex(Suit, MeldId + 0);
        yield return Tile.FromSuitAndIndex(Suit, MeldId + 1);
        yield return Tile.FromSuitAndIndex(Suit, MeldId + 2);
      }
      else if (MeldId < 16)
      {
        yield return Tile.FromSuitAndIndex(Suit, MeldId - 7);
        yield return Tile.FromSuitAndIndex(Suit, MeldId - 7);
        yield return Tile.FromSuitAndIndex(Suit, MeldId - 7);
      }
      else
      {
        yield return Tile.FromSuitAndIndex(Suit, MeldId - 16);
        yield return Tile.FromSuitAndIndex(Suit, MeldId - 16);
        yield return Tile.FromSuitAndIndex(Suit, MeldId - 16);
        yield return Tile.FromSuitAndIndex(Suit, MeldId - 16);
      }
    }
  }
}