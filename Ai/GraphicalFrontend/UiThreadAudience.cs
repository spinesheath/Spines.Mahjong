using System;
using System.Windows;
using Game.Shared;

namespace GraphicalFrontend
{
  internal class UiThreadAudience : Audience
  {
    public UiThreadAudience(params ISpectator[] members)
      : base (members)
    {
      
    }

    protected override void Broadcast(Action<ISpectator> action)
    {
      Application.Current.Dispatcher.Invoke(() => base.Broadcast(action));
    }
  }
}
