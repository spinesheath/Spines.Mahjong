using System.Threading.Tasks;

namespace GraphicalFrontend.GameEngine
{
  internal class Daiminkan : State
  {
    private readonly int _seatIndex;

    public Daiminkan(int seatIndex)
    {
      _seatIndex = seatIndex;
    }

    public override void Update(Board board)
    {
      var calledTile = board.CurrentDiscard!;
      
      ClearCurrentDiscard(board);
      
      board.ActiveSeatIndex = _seatIndex;
      var seat = board.ActiveSeat;
      seat.Hand.Daiminkan(calledTile.TileType);
      seat.ConcealedTiles.RemoveAll(t => t.TileType == calledTile.TileType);
      seat.Melds.Add(Client.Meld.Daiminkan(calledTile));
    }

    public override Task Decide(Board board, Decider decider)
    {
      return Task.CompletedTask;
    }

    public override State Advance()
    {
      return new DaiminkanDraw();
    }
  }
}