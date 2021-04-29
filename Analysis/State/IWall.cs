using System.Collections.Generic;

namespace Spines.Mahjong.Analysis.State
{
  public interface IWall
  {
    int RemainingDraws { get; }

    IEnumerable<Tile> DoraIndicators { get; }
  }
}