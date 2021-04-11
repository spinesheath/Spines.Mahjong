using System;

namespace GraphicalFrontend.Client
{
  [Flags]
  internal enum DrawActions
  {
    Discard = 0,
    Kan = 2,
    Tsumo = 16,
    Riichi = 32,
    KyuushuKyuuhai = 64
  }
}