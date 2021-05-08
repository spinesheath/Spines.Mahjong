using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal abstract class State
  {
    public virtual bool IsFinal { get; } = false;

    public abstract State Advance();

    public virtual Task Decide(Board board, Decider decider)
    {
      return Task.CompletedTask;
    }

    public abstract void Update(Board board, Wall wall);
  }
}