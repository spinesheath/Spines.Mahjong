using System;
using Game.Shared;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class EndMatch : State
  {
    public override bool IsFinal { get; } = true;

    public override State Advance()
    {
      throw new InvalidOperationException();
    }

    public override void Update(Board board, Wall wall)
    {
      throw new InvalidOperationException();
    }
  }
}