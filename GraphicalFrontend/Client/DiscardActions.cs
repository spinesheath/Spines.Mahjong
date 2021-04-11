using System;

namespace GraphicalFrontend.Client
{
  [Flags]
  internal enum DiscardActions
  {
    Pass = 0,
    Pon = 1,
    Kan = 2,
    Chii = 4,
    Ron = 8
  }
}