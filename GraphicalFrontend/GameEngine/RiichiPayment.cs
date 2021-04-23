namespace GraphicalFrontend.GameEngine
{
  internal class RiichiPayment : State
  {
    private readonly State _nextState;
    private readonly int _seatIndex;

    public RiichiPayment(State nextState, int seatIndex)
    {
      _nextState = nextState;
      _seatIndex = seatIndex;
    }

    public override State Advance()
    {
      return _nextState;
    }

    public override void Update(Board board)
    {
      board.Seats[_seatIndex].Score -= 1000;
      board.RiichiSticks += 1;
    }
  }
}