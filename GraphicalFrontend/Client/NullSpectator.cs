using GraphicalFrontend.GameEngine;

namespace GraphicalFrontend.Client
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

    public void Updated(IGameState state)
    {
    }

    public void Updated(VisibleBoard board)
    {
    }
  }
}