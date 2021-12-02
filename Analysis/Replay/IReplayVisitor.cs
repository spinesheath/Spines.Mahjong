using System.Collections.Generic;
using Spines.Mahjong.Analysis.State;

namespace Spines.Mahjong.Analysis.Replay
{
  public interface IReplayVisitor
  {
    void Ankan(int who, TileType tileType)
    {
    }

    void Chii(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
    }

    void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2)
    {
    }

    void DeclareRiichi(int who)
    {
    }

    void Discard(int seatIndex, Tile tile)
    {
    }

    void Dora(Tile tile)
    {
    }

    void Draw(int seatIndex, Tile tile)
    {
    }

    void End()
    {
    }

    void EndMatch()
    {
    }

    void GameType(GameTypeFlag flags)
    {
    }

    void Haipai(int seatIndex, Tile[] tiles)
    {
    }

    void Nuki(int who, Tile tile)
    {
    }

    void Oya(int seatIndex)
    {
    }

    void PayRiichi(int who)
    {
    }

    void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
    }

    void Ron(int who, int fromWho, PaymentInformation payment)
    {
    }

    void Ryuukyoku(RyuukyokuType ryuukyokuType, int honba, int riichiSticks, IReadOnlyList<int> scores, IReadOnlyList<int> scoreChanges)
    {
    }

    void Scores(IEnumerable<int> scores)
    {
    }

    void Seed(TileType roundWind, int honba, int riichiSticks, int dice0, int dice1, Tile doraIndicator)
    {
    }

    void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1)
    {
    }

    void Tsumo(int who, PaymentInformation payment)
    {
    }
  }
}