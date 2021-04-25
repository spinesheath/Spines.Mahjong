using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Shared
{
  public class Audience : ISpectator
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

    public void Updated(VisibleBoard board)
    {
      Broadcast(s => s.Updated(board));
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