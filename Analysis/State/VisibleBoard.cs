using System.Collections.Generic;

namespace Spines.Mahjong.Analysis.State
{
  public class VisibleBoard
  {
    private readonly Board _board;
    private readonly int _playerIndex;

    /// <summary>
    /// Shows the entire board including all private information.
    /// </summary>
    public VisibleBoard(Board board)
    {
      _board = board;
      _playerIndex = 0;
      var players = new List<VisiblePlayer>();
      for (var i = 0; i < 4; i++)
      {
        players.Add(new VisiblePlayer(board.Seats[i], true));
      }

      Seats = players;
    }

    /// <summary>
    /// A single player's view on the board. All data is transformed such that this player is at index 0.
    /// </summary>
    public VisibleBoard(Board board, int playerIndex)
    {
      _board = board;
      _playerIndex = playerIndex;
      var players = new List<VisiblePlayer>();
      for (var i = 0; i < 4; i++)
      {
        var showConcealedHand = i == 0;
        players.Add(new VisiblePlayer(board.Seats[(i + playerIndex) % 4], showConcealedHand));
      }

      Seats = players;
    }

    public int RemainingDraws => _board.Wall.RemainingDraws;

    public IEnumerable<Tile> DoraIndicators => _board.Wall.DoraIndicators;

    public int Honba => _board.Honba;

    public int RiichiSticks => _board.RiichiSticks;

    public TileType RoundWind => _board.RoundWind;

    public Tile? CurrentDiscard => _board.Seats[_board.ActiveSeatIndex].CurrentDiscard;

    public int ActivePlayerIndex => (4 + _board.ActiveSeatIndex - _playerIndex) % 4;
    
    public IReadOnlyList<VisiblePlayer> Seats { get; }

    public VisiblePlayer Watashi => Seats[0];

    public VisiblePlayer Shimocha => Seats[1];
    
    public VisiblePlayer Toimen => Seats[2];

    public VisiblePlayer Kamicha => Seats[3];
  }
}