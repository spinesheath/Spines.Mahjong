using System.Collections.Generic;
using System.Linq;

namespace Spines.Mahjong.Analysis.Replay
{
  internal class Pond
  {
    private List<PondTile> _tiles = new List<PondTile>();

    public IReadOnlyList<PondTile> Tiles => _tiles;

    public Pond Discard(int tile, bool tsumogiri)
    {
      var t = Tiles.ToList();
      t.Add(new PondTile(tile, tsumogiri));
      return new Pond {_tiles = t};
    }

    public Pond Call(int playerId)
    {
      var c = Clone();
      c._tiles = c.Tiles.ToList();
      c._tiles[^1] = c.Tiles[^1].Call(playerId);
      return c;
    }

    private Pond Clone()
    {
      var c = new Pond
      {
        _tiles = _tiles
      };
      return c;
    }
  }
}