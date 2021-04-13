using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.Client
{
  internal class DiscardedTile
  {
    public DiscardedTile(Tile tile)
    {
      Tile = tile;
    }

    public Tile Tile { get; set; }

    public bool IsTsumogiri { get; set; }

    public bool IsRiichi { get; set; }
    
    public bool IsCalled { get; set; }
  }
}