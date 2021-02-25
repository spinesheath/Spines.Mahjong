using System.Collections.Generic;

namespace Spines.Mahjong.Analysis.Shanten
{
  public interface IHandCalculator
  {
    /// <summary>
    /// The current shanten of the hand.
    /// </summary>
    int Shanten { get; }

    void Ankan(TileType tileType);
    void Chii(TileType lowestTileType, TileType calledTileType);
    IHandCalculator Clone();
    void Daiminkan(TileType tileType);
    void Discard(TileType tileType);
    void Draw(TileType tileType);

    /// <summary>
    /// All tileTypeIds that would make the hand furiten if discarded.
    /// </summary>
    IEnumerable<TileType> GetFuritenTileTypes();

    /// <summary>
    /// 34 ints, one per tileType. -1 if that tileType is not an ukeIre. 0-4 for the remaining tiles of that tileType if
    /// ukeIre.
    /// </summary>
    int[] GetUkeIreFor13();

    void Init(IEnumerable<TileType> tiles);

    /// <summary>
    /// Does ukeIre before the draw differ from ukeIre after ankan?
    /// </summary>
    bool IsUkeIreChangedByAnkan(TileType lastDrawTileType, TileType kanTileType);

    void Pon(TileType tileType);
    int ShantenAfterDiscard(TileType tileType);
    int ShantenWithTile(TileType tileType);
    void Shouminkan(TileType tileType);
  }
}