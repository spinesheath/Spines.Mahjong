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
      Yaku.BakazeNan | 
      Yaku.BakazeShaa | 
      Yaku.BakazePei | 
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
      Yaku.Ryuuiisou |
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

    private const Yaku YakumanFilter =
      Yaku.Daisangen |
      Yaku.Shousuushii |
      Yaku.Daisuushii |
      Yaku.Suuankou |
      Yaku.SuuankouTanki |
      Yaku.KokushiMusou |
      Yaku.KokushiMusouJuusanMen |
      Yaku.Tsuuiisou |
      Yaku.Chinroutou |
      Yaku.Suukantsu |
      Yaku.Ryuuiisou |
      Yaku.ChuurenPoutou |
      Yaku.Renhou |
      Yaku.Chiihou |
      Yaku.Tenhou |
      Yaku.JunseiChuurenPoutou;

    public ScoreCalculatingVisitor()
    {
      _wall = new FakeWall();
      _board = new Board(_wall);
    }

    public int CalculationCount { get; private set; }

    public int FailureCount { get; private set; }

    public Yaku WeirdYakuCollector { get; private set; }

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
      var seat = _board.Seats[seatIndex];
      seat.Discard(tile);

      var hand = seat.Hand;
      if (hand.Shanten == 0)
      {
        var ukeire = hand.GetUkeIreFor13();
        for (var i = 0; i < ukeire.Length; i++)
        {
          if (ukeire[i] > 0)
          {
            var draw = TileType.FromTileTypeId(i);
            var h = (HandCalculator) hand.WithTile(draw);
            var roundWind = _board.RoundWind.Index;
            var seatWind = seat.SeatWind.Index;
            var (tsumoYaku, tsumoFu) = YakuCalculator.Tsumo(h, draw, roundWind, seatWind);
            var (ronYaku, runFu) = YakuCalculator.Ron(h, draw, roundWind, seatWind);
            WeirdYakuCollector ^= tsumoYaku | ronYaku;
          }
        }
      }
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
      if ((payment.Yaku & ExternalYaku & YakumanFilter) != 0)
      {
        return;
      }

      CalculationCount += 1;

      var seat = _board.Seats[who];
      if (_currentShouminkanTile == null)
      {
        var discard = _board.CurrentDiscard!.TileType;
        var hand = (HandCalculator)seat.Hand.WithTile(discard);
        var roundWind = _board.RoundWind.Index;
        var seatWind = seat.SeatWind.Index;
        var (yaku, fu) = YakuCalculator.Ron(hand, discard, roundWind, seatWind);

        if (yaku != (payment.Yaku & YakuFilter))
        {
          FailureCount += 1;
        }

        var han = Han(yaku);
        if (han < 5 && fu != payment.Fu)
        {
          FailureCount += 1;
        }
      }
      else
      {
        var discard = _currentShouminkanTile.TileType;
        var hand = (HandCalculator)seat.Hand.WithTile(discard);
        var roundWind = _board.RoundWind.Index;
        var seatWind = seat.SeatWind.Index;
        var (yaku, fu) = YakuCalculator.Chankan(hand, discard, roundWind, seatWind);

        if (yaku != (payment.Yaku & YakuFilter))
        {
          FailureCount += 1;
        }

        var han = Han(yaku);
        if (han < 5 && fu != payment.Fu)
        {
          FailureCount += 1;
        }
      }
    }

    public void Tsumo(int who, PaymentInformation payment)
    {
      if ((payment.Yaku & ExternalYaku & YakumanFilter) != 0)
      {
        return;
      }

      CalculationCount += 1;

      var seat = _board.Seats[who];
      var hand = seat.Hand;
      var draw = seat.CurrentDraw!.TileType;
      var roundWind = _board.RoundWind.Index;
      var seatWind = seat.SeatWind.Index;
      var (yaku, fu) = YakuCalculator.Tsumo(hand, draw, roundWind, seatWind);

      if (yaku != (payment.Yaku & YakuFilter))
      {
        FailureCount += 1;
      }

      var han = Han(yaku);
      if (han < 5 && fu != payment.Fu)
      {
        FailureCount += 1;
      }
    }

    private int Han(Yaku yaku)
    {
      if ((yaku & YakumanFilter) != Yaku.None)
      {
        return int.MaxValue;
      }

      var setBits1 = (int)System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount((ulong)(yaku & HanMask1));
      var setBits2 = (int)System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount((ulong)(yaku & HanMask2));
      var setBits4 = (int)System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount((ulong)(yaku & HanMask4));
      return setBits1 + 2 * setBits2 + 4 * setBits4;
    }

    private static readonly Yaku HanMask1 =
      Yaku.MenzenTsumo |
      Yaku.Riichi |
      Yaku.Ippatsu |
      Yaku.Chankan |
      Yaku.RinshanKaihou |
      Yaku.HaiteiRaoyue |
      Yaku.HouteiRaoyui |
      Yaku.Pinfu |
      Yaku.OpenTanyao |
      Yaku.ClosedTanyao |
      Yaku.Iipeikou |
      Yaku.JikazeTon |
      Yaku.JikazeShaa |
      Yaku.JikazeNan |
      Yaku.JikazePei |
      Yaku.BakazeTon |
      Yaku.BakazeShaa |
      Yaku.BakazeNan |
      Yaku.BakazePei |
      Yaku.Haku |
      Yaku.Hatsu |
      Yaku.Chun |
      Yaku.OpenChanta |
      Yaku.OpenIttsuu |
      Yaku.OpenSanshokuDoujun |

      Yaku.Ryanpeikou |
      Yaku.ClosedJunchan |
      Yaku.ClosedHonitsu |

      Yaku.OpenChinitsu |

      Yaku.Renhou |
      Yaku.Tenhou |
      Yaku.Chiihou |
      Yaku.Daisangen |
      Yaku.Suuankou |
      Yaku.SuuankouTanki |
      Yaku.Tsuuiisou |
      Yaku.Ryuuiisou |
      Yaku.Chinroutou |
      Yaku.ChuurenPoutou |
      Yaku.JunseiChuurenPoutou |
      Yaku.KokushiMusou |
      Yaku.KokushiMusouJuusanMen |
      Yaku.Daisuushii |
      Yaku.Shousuushii |
      Yaku.Suukantsu;

    private static readonly Yaku HanMask2 =
      Yaku.DoubleRiichi |
      Yaku.Chiitoitsu |
      Yaku.ClosedChanta |
      Yaku.ClosedIttsuu |
      Yaku.ClosedSanshokuDoujun |
      Yaku.SanshokuDoukou |
      Yaku.Sankantsu |
      Yaku.Toitoihou |
      Yaku.Sanankou |
      Yaku.Shousangen |
      Yaku.Honroutou |

      Yaku.Ryanpeikou |
      Yaku.OpenJunchan |
      Yaku.ClosedJunchan |
      Yaku.OpenHonitsu |
      Yaku.ClosedHonitsu |

      Yaku.ClosedChinitsu;

    private static readonly Yaku HanMask4 =
      Yaku.OpenChinitsu |
      Yaku.ClosedChinitsu;

    private Board _board;
    private Tile? _currentShouminkanTile;
    private FakeWall _wall;
  }
}