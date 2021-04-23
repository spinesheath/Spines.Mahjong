using System;

namespace GraphicalFrontend.GameEngine
{
  internal class DoraIndicator : State
  {
    private readonly State _nextState;

    public DoraIndicator(State nextState)
    {
      _nextState = nextState;
    }

    public override State Advance()
    {
      return _nextState;
    }

    public override void Update(Board board)
    {
      board.Wall.RevealDoraIndicator();
    }
  }
}