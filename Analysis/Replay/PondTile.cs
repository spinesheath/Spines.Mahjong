using System.Text;

namespace Spines.Mahjong.Analysis.Replay
{
  internal class PondTile
  {
    public PondTile(int id, bool tsumogiri)
    {
      _id = id;
      _tsumogiri = tsumogiri;
    }

    private readonly int _id;
    private readonly bool _tsumogiri;
    private int? _calledBy;

    public int Id => _id;

    public PondTile Call(int playerId)
    {
      return new PondTile(Id, _tsumogiri)
      {
        _calledBy = playerId
      };
    }

    public override string ToString()
    {
      var sb = new StringBuilder();
      sb.Append(_id / 4 % 9 + 1);
      sb.Append("mpsz"[_id / 4 / 9]);
      return sb.ToString();
    }
  }
}