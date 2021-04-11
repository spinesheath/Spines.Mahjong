using System.Collections.Generic;

namespace Spines.Mahjong.Analysis.Shanten
{
  public interface IHandAnalysis
  {
    int Shanten { get; }

    /// <summary>
    /// All tileTypeIds that would make the hand furiten if discarded.
    /// </summary>
    IEnumerable<TileType> GetFuritenTileTypes();

    /// <summary>
    /// 34 ints, one per tileType. -1 if that tileType is not an ukeIre. 0-4 for the remaining tiles of that tileType if
    /// ukeIre.
    /// </summary>
    int[] GetUkeIreFor13();

    /// <summary>
    /// Does ukeIre before the draw differ from ukeIre after ankan?
    /// </summary>
    bool IsUkeIreChangedByAnkan(TileType lastDrawTileType, TileType kanTileType);

    int ShantenAfterDiscard(TileType tileType);

    int ShantenWithTile(TileType tileType);
  }
}