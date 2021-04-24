using System;

namespace GraphicalFrontend.GameEngine
{
  internal class EndMatch : State
  {
    public override bool IsFinal { get; } = true;

    public override State Advance()
    {
      throw new InvalidOperationException();
    }

    public override void Update(Board board)
    {
      throw new InvalidOperationException();
    }
  }
}