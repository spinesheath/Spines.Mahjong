namespace GraphicalFrontend.GameEngine
{
  internal class Tsumo : State
  {
    private State? _nextState;

    public override State Advance()
    {
      return _nextState!;
    }

    public override void Update(Board board)
    {
      // TODO calculate score

      var paymentInformation = new PaymentInformation();
      paymentInformation.ScoreChanges[board.ActiveSeatIndex] = 1000;
      _nextState = new Payment(new EndGame(new [] {board.ActiveSeatIndex}), paymentInformation);
    }
  }
}