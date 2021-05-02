using System.Collections.Generic;
using System.Text;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.State;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class DebugVisitor : IReplayVisitor
  {
    private readonly StringBuilder _sb = new StringBuilder();

    public void Discard(int seatIndex, Tile tile)
    {
      _sb.AppendLine($"Discard {seatIndex}: {tile}");
    }

    public void Ankan(int who, TileType tileType)
    {
      _sb.AppendLine($"Ankan {who}: {tileType}");
    }

    public void Ron(int who, int fromWho, PaymentInformation payment)
    {
      _sb.AppendLine($"Ron {who} from {fromWho}");
    }

    public void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _sb.AppendLine($"Pon {who} from {fromWho}: {calledTile}{handTile0}{handTile1}");
    }

    public void Draw(int seatIndex, Tile tile)
    {
      _sb.AppendLine($"Draw {seatIndex}: {tile}");
    }

    public void Chii(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _sb.AppendLine($"Chii {who} from {fromWho}: {calledTile}{handTile0}{handTile1}");
    }

    public void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2)
    {
      _sb.AppendLine($"Daiminkan {who} from {fromWho}: {calledTile}{handTile0}{handTile1}{handTile2}");
    }

    public void Oya(int seatIndex)
    {
      _sb.AppendLine($"Oya {seatIndex}");
    }

    public void Tsumo(int who, PaymentInformation payment)
    {
      _sb.AppendLine($"Tsumo {who}");
    }

    public void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1)
    {
      _sb.AppendLine($"Shouminkan {who} from {fromWho}: {calledTile}{addedTile}{handTile0}{handTile1}");
    }

    public void End()
    {
      _sb.AppendLine($"End");
    }

    public void EndMatch()
    {
      _sb.Clear();
    }

    public void DeclareRiichi(int who)
    {
      _sb.AppendLine($"Riichi {who}");
    }

    public void Dora(Tile tile)
    {
      _sb.AppendLine($"Dora {tile}");
    }

    public void Haipai(int seatIndex, IEnumerable<Tile> tiles)
    {
      _sb.AppendLine($"Haipai {seatIndex}: {string.Join("", tiles)}");
    }

    public void Nuki(int who, Tile tile)
    {
      throw new System.NotImplementedException();
    }

    public void PayRiichi(int who)
    {
      _sb.AppendLine($"Pay Riichi {who}");
    }

    public void Ryuukyoku(RyuukyokuType ryuukyokuType, int honba, int riichiSticks, IReadOnlyList<int> scores, IReadOnlyList<int> scoreChanges)
    {
      _sb.AppendLine($"Ryuukyoku");
    }

    public void Scores(IEnumerable<int> scores)
    {
      _sb.AppendLine($"Scores {string.Join(", ", scores)}");
    }

    public void Seed(TileType roundWind, int honba, int riichiSticks, int dice0, int dice1, Tile doraIndicator)
    {
      _sb.AppendLine($"Seed {roundWind}, honba {honba}, riichi {riichiSticks}, dice {dice0}{dice1}, dora {doraIndicator}");
    }

    public void GameType(GameTypeFlag flags)
    {
      _sb.AppendLine($"GameType {flags}");
    }
  }
}
