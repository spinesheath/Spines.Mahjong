using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.Client
{
  internal interface IClient
  {
    void Discard(Tile tile);

    void Ankan(TileType tileType);

    void Shouminkan(Tile tile);

    void Tsumo();

    void Riichi(Tile tile);

    void KyuushuKyuuhai();

    void Pass();

    void Daiminkan();

    void Pon(Tile tile0, Tile tile1, Tile discardAfterCall);

    void Chii(Tile tile0, Tile tile1, Tile discardAfterCall);

    void Ron();
  }
}
