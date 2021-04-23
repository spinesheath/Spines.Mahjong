using System.Linq;
using System.Threading.Tasks;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.GameEngine
{
  internal class Chii : State
  {
    private readonly int _seatIndex;
    private readonly Tile _tile0;
    private readonly Tile _tile1;
    private readonly Tile _discardAfterCall;

    public Chii(int seatIndex, Tile tile0, Tile tile1, Tile discardAfterCall)
    {
      _seatIndex = seatIndex;
      _tile0 = tile0;
      _tile1 = tile1;
      _discardAfterCall = discardAfterCall;
    }

    public override void Update(Board board)
    {
      var calledTile = board.CurrentDiscard!;
      var tiles = new[] {_tile0, _tile1, calledTile};
      var lowestTileType = tiles.Select(t => t.TileType).OrderBy(t => t.TileTypeId).First();

      ClearCurrentDiscard(board);

      board.ActiveSeatIndex = _seatIndex;
      var seat = board.ActiveSeat;
      seat.Hand.Chii(lowestTileType, calledTile.TileType);
      seat.ConcealedTiles.Remove(_tile0);
      seat.ConcealedTiles.Remove(_tile1);
      seat.Melds.Add(Client.Meld.Chii(tiles, calledTile));
    }

    public override Task Decide(Board board, Decider decider)
    {
      return Task.CompletedTask;
    }

    public override State Advance()
    {
      return new Discard(_discardAfterCall);
    }
  }
}