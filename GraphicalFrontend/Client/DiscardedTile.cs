namespace GraphicalFrontend.Client
{
  internal class DiscardedTile
  {
    public int TileId { get; set; }

    public bool IsTsumogiri { get; set; }

    public bool IsRiichi { get; set; }
    
    public bool IsCalled { get; set; }
  }
}