﻿using System.Collections.Generic;
using System.Linq;

namespace Spines.Mahjong.Analysis.Score
{
  public static class ScoreCalculator
  {
    private static (ScoringFieldYaku, int) CalculateInternal(IScoringData data, TileType winningTile, bool isRon, int roundWind, int seatWind)
    {
      var (yaku, fu) = ScoreLookup.Flags(data, winningTile, isRon, roundWind, seatWind);
      return ((ScoringFieldYaku)yaku, fu);
    }

    public static (int, int) Ron(IScoringData data, TileType winningTile, int roundWind, int seatWind)
    {
      var (yaku, fu) = CalculateInternal(data, winningTile, true, roundWind, seatWind);
      var han = Han.Calculate(yaku);
      return (han, fu);
    }

    public static (int, int) Tsumo(IScoringData data, TileType winningTile, int roundWind, int seatWind)
    {
      var (yaku, fu) = CalculateInternal(data, winningTile, false, roundWind, seatWind);
      var han = Han.Calculate(yaku);
      return (han, fu);
    }

    public static (int, int) Chankan(IScoringData data, TileType winningTile, int roundWind, int seatWind)
    {
      var (yaku, fu) = CalculateInternal(data, winningTile, true, roundWind, seatWind);
      var han = Han.Calculate(yaku);
      return (han, fu);
    }

    public static (Yaku, int) RonWithYaku(IScoringData data, TileType winningTile, int roundWind, int seatWind)
    {
      var (yaku, fu) = CalculateInternal(data, winningTile, true, roundWind, seatWind);
      return (MapFlags(yaku), fu);
    }

    public static (Yaku, int) TsumoWithYaku(IScoringData data, TileType winningTile, int roundWind, int seatWind)
    {
      var (yaku, fu) = CalculateInternal(data, winningTile, false, roundWind, seatWind);
      return (MapFlags(yaku), fu);
    }

    public static (Yaku, int) ChankanWithYaku(IScoringData data, TileType winningTile, int roundWind, int seatWind)
    {
      var (yaku, fu) = CalculateInternal(data, winningTile, true, roundWind, seatWind);
      return (MapFlags(yaku), fu);
    }

    private static Yaku MapFlags(ScoringFieldYaku flags)
    {
      return Map.Where(pair => flags.HasFlag(pair.Key)).Aggregate(Yaku.None, (current, pair) => current | pair.Value);
    }

    private static readonly Dictionary<ScoringFieldYaku, Yaku> Map = new()
    {
      { ScoringFieldYaku.None, Yaku.None },
      { ScoringFieldYaku.MenzenTsumo, Yaku.MenzenTsumo },
      { ScoringFieldYaku.Pinfu, Yaku.Pinfu },
      { ScoringFieldYaku.OpenTanyao, Yaku.OpenTanyao },
      { ScoringFieldYaku.ClosedTanyao, Yaku.ClosedTanyao },
      { ScoringFieldYaku.Iipeikou, Yaku.Iipeikou },
      { ScoringFieldYaku.JikazeTon, Yaku.JikazeTon },
      { ScoringFieldYaku.JikazeShaa, Yaku.JikazeShaa },
      { ScoringFieldYaku.JikazeNan, Yaku.JikazeNan },
      { ScoringFieldYaku.JikazePei, Yaku.JikazePei },
      { ScoringFieldYaku.BakazeTon, Yaku.BakazeTon },
      { ScoringFieldYaku.BakazeShaa, Yaku.BakazeShaa },
      { ScoringFieldYaku.BakazeNan, Yaku.BakazeNan },
      { ScoringFieldYaku.BakazePei, Yaku.BakazePei },
      { ScoringFieldYaku.Haku, Yaku.Haku },
      { ScoringFieldYaku.Hatsu, Yaku.Hatsu },
      { ScoringFieldYaku.Chun, Yaku.Chun },
      { ScoringFieldYaku.Chiitoitsu, Yaku.Chiitoitsu },
      { ScoringFieldYaku.OpenChanta, Yaku.OpenChanta },
      { ScoringFieldYaku.ClosedChanta, Yaku.ClosedChanta },
      { ScoringFieldYaku.OpenIttsuu, Yaku.OpenIttsuu },
      { ScoringFieldYaku.ClosedIttsuu, Yaku.ClosedIttsuu },
      { ScoringFieldYaku.OpenDoujun, Yaku.OpenSanshokuDoujun },
      { ScoringFieldYaku.ClosedDoujun, Yaku.ClosedSanshokuDoujun },
      { ScoringFieldYaku.Doukou, Yaku.SanshokuDoukou },
      { ScoringFieldYaku.Sankantsu, Yaku.Sankantsu },
      { ScoringFieldYaku.Toitoi, Yaku.Toitoihou },
      { ScoringFieldYaku.Sanankou, Yaku.Sanankou },
      { ScoringFieldYaku.Shousangen, Yaku.Shousangen },
      { ScoringFieldYaku.Honroutou, Yaku.Honroutou },
      { ScoringFieldYaku.Ryanpeikou, Yaku.Ryanpeikou },
      { ScoringFieldYaku.OpenJunchan, Yaku.OpenJunchan },
      { ScoringFieldYaku.ClosedJunchan, Yaku.ClosedJunchan },
      { ScoringFieldYaku.OpenHonitsu, Yaku.OpenHonitsu },
      { ScoringFieldYaku.ClosedHonitsu, Yaku.ClosedHonitsu },
      { ScoringFieldYaku.OpenChinitsu, Yaku.OpenChinitsu },
      { ScoringFieldYaku.ClosedChinitsu, Yaku.ClosedChinitsu },
      { ScoringFieldYaku.Daisangen, Yaku.Daisangen },
      { ScoringFieldYaku.Suuankou, Yaku.Suuankou },
      { ScoringFieldYaku.SuuankouTanki, Yaku.SuuankouTanki },
      { ScoringFieldYaku.Tsuuiisou, Yaku.Tsuuiisou },
      { ScoringFieldYaku.Ryuuiisou, Yaku.Ryuuiisou },
      { ScoringFieldYaku.Chinroutou, Yaku.Chinroutou },
      { ScoringFieldYaku.ChuurenPoutou, Yaku.ChuurenPoutou },
      { ScoringFieldYaku.JunseiChuurenPoutou, Yaku.JunseiChuurenPoutou },
      { ScoringFieldYaku.KokushiMusou, Yaku.KokushiMusou },
      { ScoringFieldYaku.KokushiMusouJuusanmen, Yaku.KokushiMusouJuusanMen },
      { ScoringFieldYaku.Daisuushii, Yaku.Daisuushii },
      { ScoringFieldYaku.Shousuushii, Yaku.Shousuushii },
      { ScoringFieldYaku.Suukantsu, Yaku.Suukantsu },
    };
  }
}