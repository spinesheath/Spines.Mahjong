using Game.Shared;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Abort : State
  {
    public override State Advance()
    {
      return new InitGame();
    }
    
    public override void Update(Board board, Wall wall)
    {
      board.Honba += 1;
    }
  }
}