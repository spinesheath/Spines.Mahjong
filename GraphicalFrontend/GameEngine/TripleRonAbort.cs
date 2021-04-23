namespace GraphicalFrontend.GameEngine
{
  internal class TripleRonAbort : State
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