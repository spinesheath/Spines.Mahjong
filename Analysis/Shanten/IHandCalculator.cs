using System.Collections.Generic;

namespace Spines.Mahjong.Analysis.Shanten
{
  public interface IHandCalculator : IHandAnalysis
  {
    void Ankan(TileType tileType);

    void Chii(TileType lowestTileType, TileType calledTileType);

    void Daiminkan(TileType tileType);

    void Discard(TileType tileType);

    void Draw(TileType tileType);

    void Init(IEnumerable<TileType> tiles);

    void Pon(TileType tileType);

    void Shouminkan(TileType tileType);
  }
}