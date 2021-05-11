using System.Collections.Generic;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.State;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class ScoreCalculatingVisitor : IReplayVisitor
  {
    public ScoreCalculatingVisitor()
    {
      _wall = new FakeWall();
      _board = new Board(_wall);
    }

    public int CalculationCount { get; private set; }

    public int FailureCount { get; private set; }

    public void Seed(TileType roundWind, int honba, int riichiSticks, int dice0, int dice1, Tile doraIndicator)
    {
      _wall = new FakeWall();
      _wall.RevealDoraIndicator(doraIndicator);
      _board = new Board(_wall);
      _board.RoundWind = roundWind;
      _board.Honba = honba;
      _board.RiichiSticks = riichiSticks;
    }

    public void Haipai(int seatIndex, IEnumerable<Tile> tiles)
    {
      var seat = _board.Seats[seatIndex];
      seat.Init(tiles);
    }

    public void Dora(Tile tile)
    {
      _wall.RevealDoraIndicator(tile);
    }

    public void Oya(int seatIndex)
    {
      _board.SetSeatWinds(seatIndex);
    }

    public void Draw(int seatIndex, Tile tile)
    {
      _board.ClearCurrentDiscard();
      _board.ActiveSeatIndex = seatIndex;
      _board.Seats[seatIndex].Draw(tile);

      _currentShouminkanTile = null;
    }

    public void Discard(int seatIndex, Tile tile)
    {
      _board.Seats[seatIndex].Discard(tile);
    }

    public void Ankan(int who, TileType tileType)
    {
      _board.Seats[who].Ankan(tileType);
    }

    public void Chii(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _board.ClearCurrentDiscard();
      _board.ActiveSeatIndex = who;
      _board.Seats[who].Chii(calledTile, handTile0, handTile1);
    }

    public void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _board.ClearCurrentDiscard();
      _board.ActiveSeatIndex = who;
      _board.Seats[who].Pon(calledTile, handTile0, handTile1);
    }

    public void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2)
    {
      _board.ClearCurrentDiscard();
      _board.ActiveSeatIndex = who;
      _board.Seats[who].Daiminkan(calledTile);
    }

    public void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1)
    {
      _currentShouminkanTile = addedTile;
      _board.Seats[who].Shouminkan(addedTile);
    }

    public void DeclareRiichi(int who)
    {
      _board.Seats[who].DeclaredRiichi = true;
    }

    public void Ron(int who, int fromWho, PaymentInformation payment)
    {
      CalculationCount += 1;

      if (_currentShouminkanTile == null)
      {
        if (!AgariValidation2.CanRon(_board, who))
        {
          FailureCount += 1;
        }
      }
      else
      {
        if (!AgariValidation2.CanChankan(_board, who, _currentShouminkanTile))
        {
          FailureCount += 1;
        }
      }
    }

    public void Tsumo(int who, PaymentInformation payment)
    {
      CalculationCount += 1;

      if ((payment.Yaku & (Yaku.Shousangen | Yaku.Daisangen | Yaku.Shousuushi | Yaku.Daisuushi)) == 0)
      {
        return;
      }

      

      // TODO rinshan
      if (!AgariValidation2.CanTsumo(_board, false))
      {
        FailureCount += 1;
      }

      var score = ScoreCalculator.Tsumo(_board, false);

    }

    private Board _board;
    private Tile? _currentShouminkanTile;
    private FakeWall _wall;
  }
}