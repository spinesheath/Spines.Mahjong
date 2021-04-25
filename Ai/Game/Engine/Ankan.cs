using System.Linq;
using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;
using Meld = Game.Shared.Meld;

namespace Game.Engine
{
  internal class Ankan : State
  {
    private readonly TileType _tileType;
    private State? _nextState;

    public Ankan(TileType tileType)
    {
      _tileType = tileType;
    }

    public override State Advance()
    {
      return _nextState!;
    }

    public override Task Decide(Board board, Decider decider)
    {
      var kans = board.Seats.SelectMany(s => s.Melds.Where(m => m.MeldType == MeldType.AddedKan || m.MeldType == MeldType.CalledKan || m.MeldType == MeldType.ClosedKan));
      if (kans.Count() == 4)
      {

      }

      _nextState = new DoraIndicator(new AnkanDraw());

      return Task.CompletedTask;
    }

    public override void Update(Board board, Wall wall)
    {
      var seat = board.ActiveSeat;

      // kokushi chankan from ankan not allowed on tenhou
      // TODO 4 kan abort for all kans: if all 4 kan by the same player, game ends with declaration of 5th kan. If 4 kans by multiple players, game ends after the discard, unless ronned

      seat.CurrentDraw = null;
      seat.Hand.Ankan(_tileType);
      seat.ConcealedTiles.RemoveAll(t => t.TileType == _tileType);
      seat.Melds.Add(Meld.Ankan(_tileType));
    }
  }
}