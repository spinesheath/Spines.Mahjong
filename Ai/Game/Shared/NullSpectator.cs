using Game.Engine;

namespace Game.Shared
{
  internal class NullSpectator : ISpectator
  {
    public void Sent(string message)
    {
    }

    public void Error(string message)
    {
    }

    public void Received(string message)
    {
    }

    public void Updated(VisibleBoard board)
    {
    }
  }
}