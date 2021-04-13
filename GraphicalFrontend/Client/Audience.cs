using System;
using System.Collections.Generic;
using System.Linq;

namespace GraphicalFrontend.Client
{
  internal class Audience : ISpectator
  {
    public Audience(params ISpectator[] members)
      : this(members.AsEnumerable())
    {

    }

    public Audience(IEnumerable<ISpectator> members)
    {
      _members = members.ToList();
    }

    public void Sent(string message)
    {
      Broadcast(s => s.Sent(message));
    }

    public void Error(string message)
    {
      Broadcast(s => s.Error(message));
    }

    public void Received(string message)
    {
      Broadcast(s => s.Received(message));
    }

    public void Updated(IGameState state)
    {
      Broadcast(s => s.Updated(state));
    }

    private readonly IReadOnlyList<ISpectator> _members;

    protected virtual void Broadcast(Action<ISpectator> action)
    {
      foreach (var spectator in _members)
      {
        action(spectator);
      }
    }
  }
}