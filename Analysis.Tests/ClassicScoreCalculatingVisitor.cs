using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.State;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class ClassicScoreCalculatingVisitor : IReplayVisitor
  {
    public ClassicScoreCalculatingVisitor()
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

    public void Haipai(int seatIndex, Tile[] tiles)
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

      var seat = _board.Seats[who];
      if (_currentShouminkanTile == null)
      {
        var discard = _board.CurrentDiscard!;
        var roundWind = _board.RoundWind.Index;
        var seatWind = seat.SeatWind.Index;
        var concealedTilesAndDiscard = seat.ConcealedTiles.Concat(new[] {discard}).ToList();
        var (yaku, fu) = ClassicScoreCalculator.Ron(discard.TileType, roundWind, seatWind, seat.Melds, concealedTilesAndDiscard);
        if ((payment.Yaku & ExternalYaku) == payment.Yaku && yaku != Yaku.None)
        {
          return;
        }

        if ((yaku & YakuFilter) != (payment.Yaku & YakuFilter) || fu != payment.Fu)
        {
          FailureCount += 1;
        }
      }
      else
      {
        var discard = _currentShouminkanTile;
        var roundWind = _board.RoundWind.Index;
        var seatWind = seat.SeatWind.Index;
        var concealedTilesAndDiscard = seat.ConcealedTiles.Concat(new[] {discard}).ToList();
        var (yaku, fu) = ClassicScoreCalculator.Chankan(discard.TileType, roundWind, seatWind, seat.Melds, concealedTilesAndDiscard);
        if ((payment.Yaku & ExternalYaku) == payment.Yaku && yaku != Yaku.None)
        {
          return;
        }

        if ((yaku & YakuFilter) != (payment.Yaku & YakuFilter) || fu != payment.Fu)
        {
          FailureCount += 1;
        }
      }
    }

    public void Tsumo(int who, PaymentInformation payment)
    {
      CalculationCount += 1;

      var seat = _board.Seats[who];
      var draw = seat.CurrentDraw!;
      var roundWind = _board.RoundWind.Index;
      var seatWind = seat.SeatWind.Index;
      var (yaku, fu) = ClassicScoreCalculator.Tsumo(draw.TileType, roundWind, seatWind, seat.Melds, seat.ConcealedTiles);
      if ((payment.Yaku & ExternalYaku) == payment.Yaku && yaku != Yaku.None)
      {
        return;
      }

      if ((yaku & YakuFilter) != (payment.Yaku & YakuFilter) || fu != payment.Fu)
      {
        FailureCount += 1;
      }
    }

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
      Yaku.ClosedDoujun |
      Yaku.OpenDoujun |
      Yaku.Doukou |
      Yaku.Toitoi |
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
      Yaku.KokushiMusouJuusanmen |
      Yaku.Pinfu |
      Yaku.Tsuuiisou |
      Yaku.Chinroutou |
      Yaku.ChuurenPoutou |
      Yaku.JunseiChuurenPoutou |
      Yaku.ClosedChanta |
      Yaku.OpenChanta |
      Yaku.Honroutou |
      Yaku.Ryuuiisou;

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
      Yaku.Houtei |
      Yaku.Haitei;

    private Board _board;
    private Tile? _currentShouminkanTile;
    private FakeWall _wall;
  }
}