using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Score
{
  public static class YakuCalculator
  {
    private static ScoringFieldYaku CalculateInternal(HandCalculator hand, Tile winningTile, bool isRon, int roundWind, int seatWind)
    {
      return (ScoringFieldYaku) ScoreLookup.Flags2(hand, winningTile, isRon, roundWind, seatWind);
    }

    public static Yaku Ron(HandCalculator hand, Tile winningTile, int roundWind, int seatWind)
    {
      return MapFlags(CalculateInternal(hand, winningTile, true, roundWind, seatWind));
    }

    public static Yaku Tsumo(HandCalculator hand, Tile winningTile, int roundWind, int seatWind)
    {
      return MapFlags(CalculateInternal(hand, winningTile, false, roundWind, seatWind));
    }

    public static Yaku Chankan(HandCalculator hand, Tile winningTile, int roundWind, int seatWind)
    {
      return MapFlags(CalculateInternal(hand, winningTile, true, roundWind, seatWind));
    }

    private static Yaku MapFlags(ScoringFieldYaku flags)
    {
      return Map.Where(pair => flags.HasFlag(pair.Key)).Aggregate(Yaku.None, (current, pair) => current | pair.Value);
    }

    private static readonly Dictionary<ScoringFieldYaku, Yaku> Map = new()
    {
      { ScoringFieldYaku.None, Yaku.None },
      //{ ScoringFieldYaku.None, Yaku.MenzenTsumo },
      //{ ScoringFieldYaku.None, Yaku.Riichi },
      //{ ScoringFieldYaku.None, Yaku.Ippatsu },
      //{ ScoringFieldYaku.None, Yaku.Chankan },
      //{ ScoringFieldYaku.None, Yaku.RinshanKaihou },
      //{ ScoringFieldYaku.None, Yaku.HaiteiRaoyue },
      //{ ScoringFieldYaku.None, Yaku.HouteiRaoyui },
      //{ ScoringFieldYaku.None, Yaku.Pinfu },
      //{ ScoringFieldYaku.None, Yaku.OpenTanyao },
      //{ ScoringFieldYaku.None, Yaku.ClosedTanyao },
      //{ ScoringFieldYaku.None, Yaku.Iipeikou },
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
      //{ ScoringFieldYaku.None, Yaku.Chiitoitsu },
      //{ ScoringFieldYaku.None, Yaku.OpenChanta },
      //{ ScoringFieldYaku.None, Yaku.ClosedChanta },
      //{ ScoringFieldYaku.None, Yaku.OpenIttsuu },
      //{ ScoringFieldYaku.None, Yaku.ClosedIttsuu },
      //{ ScoringFieldYaku.None, Yaku.OpenSanshokuDoujun },
      //{ ScoringFieldYaku.None, Yaku.ClosedSanshokuDoujun },
      //{ ScoringFieldYaku.None, Yaku.SanshokuDoukou },
      //{ ScoringFieldYaku.None, Yaku.Sankantsu },
      //{ ScoringFieldYaku.None, Yaku.Toitoihou },
      //{ ScoringFieldYaku.None, Yaku.Sanankou },
      { ScoringFieldYaku.Shousangen, Yaku.Shousangen },
      //{ ScoringFieldYaku.None, Yaku.Honroutou },
      //{ ScoringFieldYaku.None, Yaku.Ryanpeikou },
      //{ ScoringFieldYaku.None, Yaku.OpenJunchan },
      //{ ScoringFieldYaku.None, Yaku.ClosedJunchan },
      //{ ScoringFieldYaku.None, Yaku.OpenHonitsu },
      //{ ScoringFieldYaku.None, Yaku.ClosedHonitsu },
      //{ ScoringFieldYaku.None, Yaku.OpenChinitsu },
      //{ ScoringFieldYaku.None, Yaku.ClosedChinitsu },

      //{ ScoringFieldYaku.None, Yaku.Renhou },
      //{ ScoringFieldYaku.None, Yaku.Tenhou },
      //{ ScoringFieldYaku.None, Yaku.Chiihou },
      { ScoringFieldYaku.Daisangen, Yaku.Daisangen },
      //{ ScoringFieldYaku.None, Yaku.Suuankou },
      //{ ScoringFieldYaku.None, Yaku.SuuankouTanki },
      //{ ScoringFieldYaku.None, Yaku.Tsuuiisou },
      //{ ScoringFieldYaku.None, Yaku.Ryuuiisou },
      //{ ScoringFieldYaku.None, Yaku.Chinroutou },
      //{ ScoringFieldYaku.None, Yaku.ChuurenPoutou },
      //{ ScoringFieldYaku.None, Yaku.JunseiChuurenPoutou },
      //{ ScoringFieldYaku.None, Yaku.KokushiMusou },
      //{ ScoringFieldYaku.None, Yaku.KokushiMusouJuusanMen },
      { ScoringFieldYaku.Daisuushi, Yaku.Daisuushi },
      { ScoringFieldYaku.Shousuushi, Yaku.Shousuushi },
      //{ ScoringFieldYaku.None, Yaku.Suukantsu },

      //{ ScoringFieldYaku.None, Yaku.Dora },
      //{ ScoringFieldYaku.None, Yaku.UraDora },
      //{ ScoringFieldYaku.None, Yaku.AkaDora },
    };
  }
}