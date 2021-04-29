using System;
using System.Windows.Threading;
using Game.Shared;
using Spines.Mahjong.Analysis.State;

namespace GraphicalFrontend.ViewModels
{
  internal class MessagesViewModel : ViewModelBase, ISpectator
  {
    public string Messages
    {
      get => _messages;
      private set
      {
        _messages = value;
        OnPropertyChanged();
      }
    }

    public void Clear()
    {
      Messages = "";
    }

    public void Sent(string message)
    {
      AppendOutgoingMessage(message);
    }

    public void Received(string message)
    {
      AppendIncomingMessage(message);
    }

    public void Updated(VisibleBoard board)
    {
    }

    public void Error(string message)
    {
      AppendIncomingMessage(message);
    }

    private string _messages = "";

    private void AppendIncomingMessage(string message)
    {
      Dispatcher.CurrentDispatcher.Invoke(() => Messages = "    " + message + Environment.NewLine + Messages);
    }

    private void AppendOutgoingMessage(string message)
    {
      Dispatcher.CurrentDispatcher.Invoke(() => Messages = message + Environment.NewLine + Messages);
    }
  }
}