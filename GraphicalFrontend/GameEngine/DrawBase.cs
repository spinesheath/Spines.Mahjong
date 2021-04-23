using System;
using System.Linq;
using System.Threading.Tasks;
using GraphicalFrontend.Client;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;

namespace GraphicalFrontend.GameEngine
{
  internal abstract class DrawBase : State, IClient
  {
    public override State Advance()
    {
      return NextState!;
    }

    public override async Task Decide(Board board, Decider decider)
    {
      var actions = GetPossibleActions(board);
      var response = await decider.OnDraw(actions, board.ActiveSeatIndex);
      response.Execute(this);
    }

    protected State? NextState { get; set; }

    public abstract void Discard(Tile tile);

    public abstract void Ankan(TileType tileType);

    public abstract void Shouminkan(Tile tile);

    public void Tsumo()
    {
      NextState = new Tsumo();
    }

    public abstract void Riichi(Tile tile);

    public abstract void KyuushuKyuuhai();

    public void Pass()
    {
      throw new InvalidOperationException();
    }

    public void Daiminkan()
    {
      throw new InvalidOperationException();
    }

    public void Pon(Tile tile0, Tile tile1, Tile discardAfterCall)
    {
      throw new InvalidOperationException();
    }

    public void Chii(Tile tile0, Tile tile1, Tile discardAfterCall)
    {
      throw new InvalidOperationException();
    }

    public void Ron()
    {
      throw new InvalidOperationException();
    }

    protected abstract DrawActions GetPossibleActions(Board board);

    protected static bool CanKan(Board board)
    {
      if (board.Wall.RemainingDraws <= 0)
      {
        return false;
      }

      var seat = board.ActiveSeat;
      var canAnkan = seat.ConcealedTiles.GroupBy(t => t.TileType).Any(g => g.Count() == 4);
      var canShouminkan = seat.Melds.Any(m => m.MeldType == MeldType.Koutsu && seat.ConcealedTiles.Any(t => t.TileType == m.LowestTile.TileType));
      return canAnkan || canShouminkan;
    }

    protected static bool CanRiichi(Board board)
    {
      var seat = board.ActiveSeat;
      return board.Wall.RemainingDraws > 3 && seat.Hand.Shanten <= 0 && seat.Melds.Count(m => m.MeldType != MeldType.ClosedKan) == 0 && !seat.DeclaredRiichi;
    }
  }
}