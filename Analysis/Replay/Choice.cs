using System.Text;

namespace Spines.Mahjong.Analysis.Replay
{
  internal class Choice
  {
    public bool IsEqualTo(Choice other)
    {
      var meldsEqual = (_meld == null && other._meld == null) || (_meld != null && other._meld != null && _meld.IsEqualTo(other._meld));
      return _riichi == other._riichi && _discardTileId == other._discardTileId && meldsEqual &&
             _agari == other._agari && _playerId == other._playerId && _pass == other._pass;
    }

    private bool _riichi;
    private int? _discardTileId;
    private Meld _meld;
    private bool _agari;
    private readonly int _playerId;
    private bool _pass;
    private bool _ryuukyoku;

    private Choice(int playerId)
    {
      _playerId = playerId;
    }

    public int? DiscardedTileId => _discardTileId;

    public bool IsDiscard => _discardTileId != null;

    public bool IsAgari => _agari;

    public bool IsCall => _meld != null;

    public int PlayerId => _playerId;

    public static Choice Riichi(int playerId, int discardTileId)
    {
      return new Choice(playerId) { _riichi = true, _discardTileId = discardTileId };
    }

    public static Choice Agari(int playerId)
    {
      return new Choice(playerId) { _agari = true };
    }

    public static Choice Call(int playerId, Meld meld)
    {
      return new Choice(playerId) { _meld = meld };
    }

    public static Choice Pass(int playerId)
    {
      return new Choice(playerId) {_pass = true};
    }

    public static Choice Discard(int playerId, int tileId)
    {
      return new Choice(playerId) { _discardTileId = tileId };
    }

    public static Choice Ryuukyoku(int playerId)
    {
      return new Choice(playerId) { _ryuukyoku = true };
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append(PlayerId);
      sb.Append(" ");
      if (_riichi)
      {
        sb.Append("riichi ");
        sb.Append(_discardTileId);
      }
      else if (IsAgari)
      {
        sb.Append("agari");
      }
      else if (_meld != null)
      {
        sb.Append("meld ");
        sb.Append(_meld);
      }
      else if (_pass)
      {
        sb.Append("pass");
      }
      else if (_ryuukyoku)
      {
        sb.Append("ryuukyoku");
      }
      else
      {
        sb.Append("discard ");
        sb.Append(_discardTileId);
      }
      return sb.ToString();
    }
  }
}