using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spines.Mahjong.Analysis.Replay
{
  internal class Meld
  {
    public Meld(List<int> tiles, int? calledTile, int? calledFrom, string name)
    {
      _tiles = tiles;
      _calledTile = calledTile;
      _calledFrom = calledFrom;
      _name = name;
    }
    
    public int? CalledTile => _calledTile;

    private readonly List<int> _tiles;
    private readonly int? _calledTile;
    private readonly int? _calledFrom;
    private readonly string _name;

    public bool IsKoutsu => _tiles.GroupBy(t => t / 4).Count() == 1;

    public bool IsShuntsu => !IsKoutsu;

    public bool IsChantaGroup
    {
      get
      {
        return Tiles.Any(t => t / 4 >= 27 || t / 4 % 9 == 0 || t / 4 % 9 == 8);
      }
    }

    public bool IsEqualTo(Meld other)
    {
      return _calledFrom == other._calledFrom && _calledTile == other._calledTile && _tiles.OrderBy(x => x).SequenceEqual(other._tiles.OrderBy(x => x));
    }

    public IReadOnlyList<int> Tiles => _tiles;

    public bool IsAnkan => Tiles.Count == 4 && _calledFrom == null && _calledTile == null;

    public static Meld Ankan(List<int> tiles)
    {
      return new Meld(tiles, null, null, "ankan");
    }

    public static Meld Shouminkan(List<int> tiles, Meld pon)
    {
      return new Meld(tiles, pon._calledTile, pon._calledFrom, "shouminkan");
    }

    public static Meld Daiminkan(List<int> tiles, int calledTile, int calledFrom)
    {
      return new Meld(tiles, calledTile, calledFrom, "daiminkan");
    }

    public static Meld Pon(List<int> tiles, int calledTile, int calledFrom)
    {
      return new Meld(tiles, calledTile, calledFrom, "pon");
    }

    public static Meld Chii(List<int> tiles, int calledTile, int calledFrom)
    {
      return new Meld(tiles, calledTile, calledFrom, "chii");
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append("[");
      if (_calledFrom != null)
      {
        sb.Append("f");
        sb.Append(_calledFrom);
      }

      if (_calledTile != null)
      {
        sb.Append("t");
        sb.Append((_calledTile / 4 % 9) + 1);
        sb.Append(" ");
      }

      foreach (var tile in _tiles)
      {
        sb.Append((tile / 4 % 9) + 1);
      }

      sb.Append("mpsz"[_tiles[0] / 4 / 9]);

      sb.Append("]");
      return sb.ToString();
    }
  }
}