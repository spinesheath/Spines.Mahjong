using Game.Shared;

namespace Game.Engine
{
  internal class FourWindsAbort : State
  {
    public override void Update(Board board, Wall wall)
    {
      board.Honba += 1;
    }

    public override State Advance()
    {
      return new InitGame();
    }
  }
}