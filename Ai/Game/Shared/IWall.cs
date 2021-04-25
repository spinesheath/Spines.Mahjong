using System.Collections.Generic;
using Spines.Mahjong.Analysis;

namespace Game.Shared
{
  internal interface IWall
  {
    int RemainingDraws { get; }

    IEnumerable<Tile> DoraIndicators { get; }
  }
}