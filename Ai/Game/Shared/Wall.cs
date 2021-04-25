using System;
using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis;

namespace Game.Shared
{
  internal class Wall : IWall
  {
    public IEnumerable<Tile> DoraIndicators => _doraIndicators;

    public int RemainingDraws => _tiles.Count - 14 + _doraIndicators.Count;

    public Tile Draw()
    {
      return _tiles.Dequeue();
    }

    public Tile DrawFromDeadWall()
    {
      return Draw();
    }

    public IEnumerable<Tile> DrawInitialHand()
    {
      return Draw(13).ToList();
    }

    public Tile RevealDoraIndicator()
    {
      var tile = _tiles.Dequeue();
      _doraIndicators.Add(tile);
      return tile;
    }

    public void Shuffle()
    {
      var tiles = Enumerable.Range(0, 136).ToList();
      _seed = (int)DateTime.Now.Ticks;
      var random = new Random(_seed);
      var n = 136;
      while (n > 1)
      {
        n--;
        var k = random.Next(n + 1);
        var value = tiles[k];
        tiles[k] = tiles[n];
        tiles[n] = value;
      }

      _tiles = new Queue<Tile>(tiles.Select(Tile.FromTileId));
      _doraIndicators = new List<Tile>();
    }

    private List<Tile> _doraIndicators = new();
    private Queue<Tile> _tiles = new();
    private int _seed;

    private IEnumerable<Tile> Draw(int count)
    {
      for (var i = 0; i < count; i++)
      {
        yield return _tiles.Dequeue();
      }
    }
  }
}