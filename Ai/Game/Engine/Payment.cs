using Game.Shared;

namespace Game.Engine
{
  internal class Payment : State
  {
    public Payment(State nextState, PaymentInformation paymentInformation)
    {
      _nextState = nextState;
      _paymentInformation = paymentInformation;
    }

    public override State Advance()
    {
      return _nextState;
    }

    public override void Update(Board board, Wall wall)
    {
      board.RiichiSticks = 0;
      for (var i = 0; i < 4; i++)
      {
        board.Seats[i].Score += _paymentInformation.ScoreChanges[i];
      }
    }

    private readonly State _nextState;
    private readonly PaymentInformation _paymentInformation;
  }
}