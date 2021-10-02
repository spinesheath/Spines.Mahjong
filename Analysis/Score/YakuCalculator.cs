using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Score
{
  public static class YakuCalculator
  {
    private static ScoringFieldYaku CalculateInternal(HandCalculator hand, TileType winningTile, bool isRon, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds)
    {
      return (ScoringFieldYaku) ScoreLookup.Flags(hand, winningTile, isRon, roundWind, seatWind, melds);
    }

    public static Yaku Ron(HandCalculator hand, TileType winningTile, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds)
    {
      return MapFlags(CalculateInternal(hand, winningTile, true, roundWind, seatWind, melds));
    }

    public static Yaku Tsumo(HandCalculator hand, TileType winningTile, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds)
    {
      return MapFlags(CalculateInternal(hand, winningTile, false, roundWind, seatWind, melds));
    }

    public static Yaku Chankan(HandCalculator hand, TileType winningTile, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds)
    {
      return MapFlags(CalculateInternal(hand, winningTile, true, roundWind, seatWind, melds));
    }

    private static Yaku MapFlags(ScoringFieldYaku flags)
    {
      return Map.Where(pair => flags.HasFlag(pair.Key)).Aggregate(Yaku.None, (current, pair) => current | pair.Value);
    }

    private static readonly Dictionary<ScoringFieldYaku, Yaku> Map = new()
    {
      { ScoringFieldYaku.None, Yaku.None },
      { ScoringFieldYaku.MenzenTsumo, Yaku.MenzenTsumo },
      //{ ScoringFieldYaku.None, Yaku.Riichi },
      //{ ScoringFieldYaku.None, Yaku.Ippatsu },
      //{ ScoringFieldYaku.None, Yaku.Chankan },
      //{ ScoringFieldYaku.None, Yaku.RinshanKaihou },
      //{ ScoringFieldYaku.None, Yaku.HaiteiRaoyue },
      //{ ScoringFieldYaku.None, Yaku.HouteiRaoyui },
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
      //{ ScoringFieldYaku.None, Yaku.DoubleRiichi },
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

      //{ ScoringFieldYaku.None, Yaku.Renhou },
      //{ ScoringFieldYaku.None, Yaku.Tenhou },
      //{ ScoringFieldYaku.None, Yaku.Chiihou },
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

      //{ ScoringFieldYaku.None, Yaku.Dora },
      //{ ScoringFieldYaku.None, Yaku.UraDora },
      //{ ScoringFieldYaku.None, Yaku.AkaDora },
    };
  }
}