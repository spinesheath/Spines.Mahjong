using System;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.Shanten;
using Spines.Mahjong.Analysis.State;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class ScoreCalculatingVisitor : IReplayVisitor
  {
    public int CalculationCount { get; private set; }

    public int FailureCount { get; private set; }

    private readonly ProgressiveScoringData[] _scoring = { new(), new(), new(), new() };

    private readonly int[][] _base5Hashes = { new int[4], new int[4], new int[4], new int[4] };

    private TileType? _roundWind;

    private int? _oyaSeatIndex;

    public void Seed(TileType roundWind, int honba, int riichiSticks, int dice0, int dice1, Tile doraIndicator)
    {
      _roundWind = roundWind;
    }

    public void Haipai(int seatIndex, Tile[] tiles)
    {
      var hashes = _base5Hashes[seatIndex];
      hashes[0] = 0;
      hashes[1] = 0;
      hashes[2] = 0;
      hashes[3] = 0;

      var t = tiles; // this variable improves performance. No idea why.
      for (var i = 0; i < t.Length; i++)
      {
        hashes[t[i].SuitId] += t[i].Base5Value;
      }

      _scoring[seatIndex] = new ProgressiveScoringData();
      _scoring[seatIndex].Init(hashes);
    }

    public void Dora(Tile tile)
    {
    }

    public void Oya(int seatIndex)
    {
      _oyaSeatIndex = seatIndex;
    }

    public void Draw(int seatIndex, Tile tile)
    {
      _currentShouminkanTile = null;
      _mostRecentDraw = tile;

      var hashes = _base5Hashes[seatIndex];
      var suitId = tile.SuitId;
      hashes[suitId] += Base5.Table[tile.Index];

      _scoring[seatIndex].Draw(suitId, hashes[suitId]);
    }

    public void Discard(int seatIndex, Tile tile)
    {
      _mostRecentDiscard = tile;

      var hashes = _base5Hashes[seatIndex];
      var suitId = tile.SuitId;
      hashes[suitId] -= Base5.Table[tile.Index];

      _scoring[seatIndex].Discard(suitId, hashes[suitId]);
    }

    public void Ankan(int who, TileType tileType)
    {
      var hashes = _base5Hashes[who];
      var suitId = tileType.SuitId;
      hashes[suitId] -= 4 * Base5.Table[tileType.Index];
      
      _scoring[who].Ankan(suitId, tileType.Index);
      _scoring[who].UpdateSuit(suitId, hashes[suitId]);
    }

    public void Chii(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      var hashes = _base5Hashes[who];
      var suitId = calledTile.SuitId;
      hashes[suitId] -= Base5.Table[handTile0.Index];
      hashes[suitId] -= Base5.Table[handTile1.Index];

      var minIndex = Math.Min(Math.Min(calledTile.Index, handTile0.Index), handTile1.Index);
      _scoring[who].Chii(suitId, minIndex);
      _scoring[who].UpdateSuit(suitId, hashes[suitId]);
    }

    public void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      var hashes = _base5Hashes[who];
      var suitId = calledTile.SuitId;
      var index = calledTile.Index;
      hashes[suitId] -= 2 * Base5.Table[index];

      _scoring[who].Pon(suitId, index);
      _scoring[who].UpdateSuit(suitId, hashes[suitId]);
    }

    public void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2)
    {
      var hashes = _base5Hashes[who];
      var suitId = calledTile.SuitId;
      var index = calledTile.Index;
      hashes[suitId] -= 3 * Base5.Table[index];

      _scoring[who].Daiminkan(suitId, index);
      _scoring[who].UpdateSuit(suitId, hashes[suitId]);
    }

    public void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1)
    {
      _currentShouminkanTile = addedTile;

      var hashes = _base5Hashes[who];
      var suitId = calledTile.SuitId;
      var index = calledTile.Index;
      hashes[suitId] -= Base5.Table[index];

      _scoring[who].Shouminkan(calledTile.TileType);
      _scoring[who].UpdateSuit(suitId, hashes[suitId]);
    }

    public void DeclareRiichi(int who)
    {
    }

    public void Ron(int who, int fromWho, PaymentInformation payment)
    {
      if (_oyaSeatIndex == null || _roundWind == null || _mostRecentDiscard == null)
      {
        throw new InvalidOperationException();
      }

      if ((payment.Yaku & ExternalYaku & YakumanFilter) != 0)
      {
        return;
      }

      CalculationCount += 1;

      var winningTile = _currentShouminkanTile ?? _mostRecentDiscard;

      var hashes = _base5Hashes[who];
      var winningSuitId = winningTile.SuitId;
      hashes[winningSuitId] += Base5.Table[winningTile.Index];

      _scoring[who].Draw(winningSuitId, hashes[winningSuitId]);

      var seatWind = (who - _oyaSeatIndex.Value + 4) % 4;
      var wind = new WindScoringData(_roundWind.Index, seatWind);

      Yaku yaku;
      int fu;

      if (_currentShouminkanTile == null)
      {
        (yaku, fu) = ScoreCalculator.RonWithYaku(_scoring[who], wind, _mostRecentDiscard.TileType);
      }
      else
      {
        (yaku, fu) = ScoreCalculator.ChankanWithYaku(_scoring[who], wind, _currentShouminkanTile.TileType);
      }

      if (yaku != (payment.Yaku & YakuFilter))
      {
        FailureCount += 1;
      }

      var han = Han.Calculate(yaku);
      if (han < 5 && fu != payment.Fu)
      {
        FailureCount += 1;
      }
    }

    public void Tsumo(int who, PaymentInformation payment)
    {
      if (_oyaSeatIndex == null || _roundWind == null || _mostRecentDraw == null)
      {
        throw new InvalidOperationException();
      }

      if ((payment.Yaku & ExternalYaku & YakumanFilter) != 0)
      {
        return;
      }

      CalculationCount += 1;

      var seatWind = (who - _oyaSeatIndex.Value + 4) % 4;
      var wind = new WindScoringData(_roundWind.Index, seatWind);
      var (yaku, fu) = ScoreCalculator.TsumoWithYaku(_scoring[who], wind, _mostRecentDraw.TileType);
      
      if (yaku != (payment.Yaku & YakuFilter))
      {
        FailureCount += 1;
      }

      var han = Han.Calculate(yaku);
      if (han < 5 && fu != payment.Fu)
      {
        FailureCount += 1;
      }
    }

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
      Yaku.Houtei |
      Yaku.Haitei;

    private const Yaku YakumanFilter =
      Yaku.Daisangen |
      Yaku.Shousuushii |
      Yaku.Daisuushii |
      Yaku.Suuankou |
      Yaku.SuuankouTanki |
      Yaku.KokushiMusou |
      Yaku.KokushiMusouJuusanmen |
      Yaku.Tsuuiisou |
      Yaku.Chinroutou |
      Yaku.Suukantsu |
      Yaku.Ryuuiisou |
      Yaku.ChuurenPoutou |
      Yaku.Renhou |
      Yaku.Chiihou |
      Yaku.Tenhou |
      Yaku.JunseiChuurenPoutou;
    
    private Tile? _currentShouminkanTile;
    private Tile? _mostRecentDiscard;
    private Tile? _mostRecentDraw;
  }
}