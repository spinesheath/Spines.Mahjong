namespace GraphicalFrontend.GameEngine
{
  internal class FourWindsAbort : State
  {
    public override void Update(Board board)
    {
      board.Honba += 1;
    }

    public override State Advance()
    {
      return new InitGame();
    }
  }
}