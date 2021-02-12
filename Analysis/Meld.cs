using System.Collections.Generic;
using Spines.Mahjong.Analysis.Shanten;

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
        yield return new Tile { Index = MeldId + 0, Suit = Suit };
        yield return new Tile { Index = MeldId + 1, Suit = Suit };
        yield return new Tile { Index = MeldId + 2, Suit = Suit };
      }
      else if (MeldId < 16)
      {
        yield return new Tile { Index = MeldId - 7, Suit = Suit };
        yield return new Tile { Index = MeldId - 7, Suit = Suit };
        yield return new Tile { Index = MeldId - 7, Suit = Suit };
      }
      else
      {
        yield return new Tile {Index = MeldId - 16, Suit = Suit};
        yield return new Tile {Index = MeldId - 16, Suit = Suit};
        yield return new Tile {Index = MeldId - 16, Suit = Suit};
        yield return new Tile {Index = MeldId - 16, Suit = Suit};
      }
    }
  }
}