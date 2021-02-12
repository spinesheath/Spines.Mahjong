using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spines.Mahjong.Analysis.Replay
{
  internal class PlayerState
  {
    public PlayerState(string name, int rank, double rate, string gender)
    {
      _name = name;
      _rank = rank;
      _rate = rate;
      _gender = gender;
    }

    private List<int> _concealedTiles = new List<int>();
    private List<Meld> _melds = new List<Meld>();
    private int _score;
    private int _seatWind;
    private readonly int _rank;
    private readonly double _rate;
    private readonly string _name;
    private readonly string _gender;
    private bool _isDisconnected;
    private int _shanten;
    private bool _justMelded;

    public bool JustMelded => _justMelded;

    public string Name => _name;

    public int Shanten => _shanten;

    public int? RecentDrawTileId { get; private set; }

    public IReadOnlyList<int> ConcealedTiles => _concealedTiles;

    public bool IsInRiichi { get; private set; }

    public IReadOnlyList<Meld> Melds => _melds;

    public PlayerState Disconnect()
    {
      var c = Clone();
      c._isDisconnected = true;
      return c;
    }

    public PlayerState Connect()
    {
      var c = Clone();
      c._isDisconnected = false;
      return c;
    }

    public PlayerState Draw(int tileId)
    {
      var c = Clone();
      c._concealedTiles = c.ConcealedTiles.ToList();
      c._concealedTiles.Add(tileId);
      c.RecentDrawTileId = tileId;
      return c;
    }

    public PlayerState Draw(IEnumerable<int> tileIds)
    {
      var c = Clone();
      c._concealedTiles = c.ConcealedTiles.ToList();
      c._concealedTiles.AddRange(tileIds);
      c.RecentDrawTileId = null;
      return c;
    }

    public PlayerState Discard(int tileId)
    {
      var c = Clone();
      c._concealedTiles = c.ConcealedTiles.ToList();
      c._concealedTiles.Remove(tileId);
      c.RecentDrawTileId = null;
      c._justMelded = false;
      return c;
    }

    public PlayerState SetShanten(int shanten)
    {
      var c = Clone();
      c._shanten = shanten;
      return c;
    }

    private PlayerState Clone()
    {
      return new PlayerState(_name, _rank, _rate, _gender)
      {
        _concealedTiles = _concealedTiles,
        _melds = _melds,
        IsInRiichi = IsInRiichi,
        _score = _score,
        _seatWind = _seatWind,
        _isDisconnected = _isDisconnected,
        _shanten = _shanten,
        RecentDrawTileId = RecentDrawTileId,
        _justMelded = _justMelded
      };
  }

    public PlayerState Init(int score)
    {
      var c = Clone();
      c.RecentDrawTileId = null;
      c.IsInRiichi = false;
      c._concealedTiles = new List<int>();
      c._melds = new List<Meld>();
      c._score = score;
      c._justMelded = false;
      return c;
    }

    public PlayerState ChangeScore(int amount)
    {
      var c = Clone();
      c._score += amount;
      return c;
    }

    public PlayerState DeclareRiichi()
    {
      var c = Clone();
      c.IsInRiichi = true;
      return c;
    }

    public PlayerState Call(List<int> tileIds, int? calledFromPlayerId, string name)
    {
      var c = Clone();
      c._concealedTiles = c.ConcealedTiles.ToList();
      c._melds = c.Melds.ToList();
      int? calledTile = null;
      foreach (var tileId in tileIds)
      {
        if (!c._concealedTiles.Remove(tileId))
          calledTile = tileId;
      }
      
      var meld = new Meld(tileIds, calledTile, calledFromPlayerId, name);
      var toUpgrade = c._melds.FirstOrDefault(m => m.Tiles.Intersect(meld.Tiles).Count() == 3);
      if (toUpgrade != null)
      {
        c._melds[c._melds.IndexOf(toUpgrade)] = meld;
      }
      else
      {
        c._melds.Add(meld);
      }

      c._justMelded = true;

      return c;
    }

    public override string ToString()
    {
      return GetConcealedString(0, 'm') + GetConcealedString(1, 'p') + GetConcealedString(2, 's') + GetConcealedString(3, 'z') +
             string.Join("", _melds);
    }

    private string GetConcealedString(int suitId, char suit)
    {
      var sb = new StringBuilder();
      var tilesInSuit = _concealedTiles.Where(t => t / 4 / 9 == suitId);
      var tiles = new int[9];
      foreach (var tileId in tilesInSuit)
      {
        tiles[tileId / 4 % 9] += 1;
      }
      for (var i = 0; i < tiles.Length; ++i)
      {
        for (var j = 0; j < tiles[i]; ++j)
        {
          sb.Append((char)('1' + i));
        }
      }
      if (sb.Length == 0)
      {
        return string.Empty;
      }
      sb.Append(suit);
      return sb.ToString();
    }
  }
}