using System;

namespace Game.Shared
{
  [Flags]
  public enum DiscardActions
  {
    Pass = 0,
    Pon = 1,
    Kan = 2,
    Chii = 4,
    Ron = 8
  }
}