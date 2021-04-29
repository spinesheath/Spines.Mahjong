using Game.Engine;
using Spines.Mahjong.Analysis.State;

namespace Game.Shared
{
  public interface ISpectator
  {
    void Sent(string message);

    void Error(string message);

    void Received(string message);

    void Updated(VisibleBoard board);
  }
}