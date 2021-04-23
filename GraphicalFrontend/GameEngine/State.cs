﻿using System.Threading.Tasks;

namespace GraphicalFrontend.GameEngine
{
  internal abstract class State
  {
    public virtual bool IsFinal { get; } = false;

    public abstract State Advance();

    public virtual Task Decide(Board board, Decider decider)
    {
      return Task.CompletedTask;
    }

    public abstract void Update(Board board);

    protected static void ClearCurrentDiscard(Board board)
    {
      foreach (var s in board.Seats)
      {
        s.CurrentDiscard = null;
      }
    }
  }
}