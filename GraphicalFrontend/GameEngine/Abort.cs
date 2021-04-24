namespace GraphicalFrontend.GameEngine
{
  internal class Abort : State
  {
    public override State Advance()
    {
      return new InitGame();
    }
    
    public override void Update(Board board)
    {
      board.Honba += 1;
    }
  }
}