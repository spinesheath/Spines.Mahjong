namespace GraphicalFrontend.GameEngine
{
  internal class KyuushuKyuuhai : State
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