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
      for (var i = 0; i < 4; i++)
      {
        paymentInformation.ScoreChanges[i] = i == board.ActiveSeatIndex ? 6000 + board.RiichiSticks * 1000 + board.Honba * 300 : -2000 - board.Honba * 100;
      }

      _nextState = new Payment(new EndGame(new [] {board.ActiveSeatIndex}), paymentInformation);
    }
  }
}