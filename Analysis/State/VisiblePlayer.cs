using System.Collections.Generic;
using System.Diagnostics;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.State
{
  public class VisiblePlayer
  {
    private readonly Seat _player;
    private readonly bool _showConcealedHand;

    internal VisiblePlayer(Seat player, bool showConcealedHand)
    {
      _player = player;
      _showConcealedHand = showConcealedHand;
    }

    public bool DeclaredRiichi => _player.DeclaredRiichi;

    public Tile? CurrentDiscard => _player.CurrentDiscard;

    public Tile? CurrentDraw
    {
      get
      {
        Debug.Assert(_showConcealedHand, "Can't look at other player's hands.");
        return _showConcealedHand ? _player.CurrentDraw : null;
      }
    }

    public IReadOnlyList<Tile> ConcealedTiles
    {
      get
      {
        Debug.Assert(_showConcealedHand, "Can't look at other player's hands.");
        return _showConcealedHand ? _player.ConcealedTiles : new List<Tile>();
      }
    }

    public UkeIreCalculator Hand
    {
      get
      {
        Debug.Assert(_showConcealedHand, "Can't look at other player's hands.");
        return _showConcealedHand ? _player.Hand : new UkeIreCalculator();
      }
    }

    public IReadOnlyList<Meld> Melds => _player.Melds;

    public TileType SeatWind => _player.SeatWind;

    public int Score => _player.Score;

    public IReadOnlyList<Tile> Discards => _player.Discards;
  }
}