using System.Collections.Generic;

namespace Spines.Mahjong.Analysis.Replay
{
  public interface IReplayVisitor
  {
    void End();

    void EndMatch();

    void GameType(GameTypeFlag flags);

    void Seed(TileType roundWind, int honba, int riichiSticks, int dice0, int dice1, Tile doraIndicator);

    void Oya(int seatIndex);

    void Scores(IEnumerable<int> scores);

    void Haipai(int seatIndex, IEnumerable<Tile> tiles);

    void Draw(int seatIndex, Tile tile);

    void Discard(int seatIndex, Tile tile);

    void Chii(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1);

    void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1);

    void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2);

    void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1);

    void Ankan(int who, TileType tileType);
  }
}