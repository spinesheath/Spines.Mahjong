using System.Collections.Generic;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.Shanten;
using Spines.Mahjong.Analysis.State;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class ScoreCalculatingVisitor : IReplayVisitor
  {
    private const Yaku YakuFilter = 
      Yaku.Haku | 
      Yaku.Hatsu | 
      Yaku.Chun | 
      Yaku.BakazeTon | 
      Yaku.JikazeNan | 
      Yaku.JikazeShaa | 
      Yaku.JikazePei | 
      Yaku.JikazeTon | 
      Yaku.JikazeNan | 
      Yaku.JikazeShaa | 
      Yaku.JikazePei |
      Yaku.Shousangen |
      Yaku.Daisangen |
      Yaku.Shousuushii |
      Yaku.Daisuushii | 
      Yaku.ClosedSanshokuDoujun | 
      Yaku.OpenSanshokuDoujun | 
      Yaku.SanshokuDoukou | 
      Yaku.Toitoihou | 
      Yaku.ClosedHonitsu | 
      Yaku.ClosedChinitsu | 
      Yaku.OpenHonitsu |
      Yaku.OpenChinitsu | 
      Yaku.ClosedTanyao | 
      Yaku.OpenTanyao | 
      Yaku.MenzenTsumo |
      Yaku.Sanankou |
      Yaku.Suuankou |
      Yaku.SuuankouTanki |
      Yaku.Iipeikou | 
      Yaku.Chiitoitsu | 
      Yaku.Ryanpeikou |
      Yaku.KokushiMusou |
      Yaku.KokushiMusouJuusanMen | 
      Yaku.Pinfu |
      Yaku.Tsuuiisou |
      Yaku.Chinroutou |
      Yaku.ClosedChanta |
      Yaku.OpenChanta |
      Yaku.Honroutou |
      Yaku.ClosedJunchan |
      Yaku.OpenJunchan |
      Yaku.ClosedIttsuu |
      Yaku.OpenIttsuu |
      Yaku.Sankantsu |
      Yaku.Suukantsu |
      Yaku.ChuurenPoutou |
      Yaku.JunseiChuurenPoutou;

    private const Yaku ExternalYaku =
      Yaku.AkaDora |
      Yaku.Dora |
      Yaku.UraDora |
      Yaku.Riichi |
      Yaku.DoubleRiichi |
      Yaku.Ippatsu |
      Yaku.Chankan |
      Yaku.Renhou |
      Yaku.Chiihou |
      Yaku.Tenhou |
      Yaku.RinshanKaihou |
      Yaku.HouteiRaoyui |
      Yaku.HaiteiRaoyue;

    private const Yaku IgnoredYaku =
      Yaku.Ryuuiisou; 

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

      if ((payment.Yaku & IgnoredYaku) != 0)
      {
        return;
      }

      var seat = _board.Seats[who];
      if (_currentShouminkanTile == null)
      {
        //if (!AgariValidation2.CanRon(_board, who))
        //{
        //  FailureCount += 1;
        //}

        var discard = _board.CurrentDiscard!;
        var hand = (HandCalculator)seat.Hand.WithTile(discard.TileType);
        var roundWind = _board.RoundWind.Index;
        var seatWind = seat.SeatWind.Index;
        var yaku = YakuCalculator.Ron(hand, discard, roundWind, seatWind, seat.Melds);
        if ((payment.Yaku & ExternalYaku) == payment.Yaku && yaku != Yaku.None)
        {
          return;
        }

        if ((yaku & YakuFilter) != (payment.Yaku & YakuFilter))
        {
          FailureCount += 1;
        }
      }
      else
      {
        //if (!AgariValidation2.CanChankan(_board, who, _currentShouminkanTile))
        //{
        //  FailureCount += 1;
        //}

        var discard = _currentShouminkanTile;
        var hand = (HandCalculator)seat.Hand.WithTile(discard.TileType);
        var roundWind = _board.RoundWind.Index;
        var seatWind = seat.SeatWind.Index;
        var yaku = YakuCalculator.Chankan(hand, discard, roundWind, seatWind, seat.Melds);
        if ((payment.Yaku & ExternalYaku) == payment.Yaku && yaku != Yaku.None)
        {
          return;
        }

        if ((yaku & YakuFilter) != (payment.Yaku & YakuFilter))
        {
          FailureCount += 1;
        }
      }
    }

    public void Tsumo(int who, PaymentInformation payment)
    {
      CalculationCount += 1;

      if ((payment.Yaku & IgnoredYaku) != 0)
      {
        return;
      }

      //if (!AgariValidation2.CanTsumo(_board, false))
      //{
      //  FailureCount += 1;
      //}

      var seat = _board.Seats[who];
      var hand = seat.Hand;
      var draw = seat.CurrentDraw!;
      var roundWind = _board.RoundWind.Index;
      var seatWind = seat.SeatWind.Index;
      var yaku = YakuCalculator.Tsumo(hand, draw, roundWind, seatWind, seat.Melds);
      if ((payment.Yaku & ExternalYaku) == payment.Yaku && yaku != Yaku.None)
      {
        return;
      }

      if ((yaku & YakuFilter) != (payment.Yaku & YakuFilter))
      {
        FailureCount += 1;
      }

      //var score = ScoreCalculator.Tsumo(_board, false);
    }

    private Board _board;
    private Tile? _currentShouminkanTile;
    private FakeWall _wall;
  }
}