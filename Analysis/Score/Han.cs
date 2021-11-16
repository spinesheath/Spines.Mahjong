using System.Runtime.Intrinsics.X86;

namespace Spines.Mahjong.Analysis.Score
{
  public static class Han
  {
    public static int Calculate(Yaku yaku)
    {
      if ((yaku & YakumanFilter) != Yaku.None)
      {
        var yakumanBits = (int) Popcnt.X64.PopCount((ulong) yaku);
        return yakumanBits + 128;
      }

      var setBits1 = (int) Popcnt.X64.PopCount((ulong) (yaku & HanMask1));
      var setBits2 = (int) Popcnt.X64.PopCount((ulong) (yaku & HanMask2));
      var setBits4 = (int) Popcnt.X64.PopCount((ulong) (yaku & HanMask4));
      return setBits1 + 2 * setBits2 + 4 * setBits4;
    }

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

    private const long ScoringFieldYakumanFilter = (long) (
      ScoringFieldYaku.Daisangen |
      ScoringFieldYaku.Shousuushii |
      ScoringFieldYaku.Daisuushii |
      ScoringFieldYaku.Suuankou |
      ScoringFieldYaku.SuuankouTanki |
      ScoringFieldYaku.KokushiMusou |
      ScoringFieldYaku.KokushiMusouJuusanmen |
      ScoringFieldYaku.Tsuuiisou |
      ScoringFieldYaku.Chinroutou |
      ScoringFieldYaku.Suukantsu |
      ScoringFieldYaku.Ryuuiisou |
      ScoringFieldYaku.ChuurenPoutou |
      ScoringFieldYaku.JunseiChuurenPoutou);

    private const Yaku HanMask1 =
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

    private const Yaku HanMask2 =
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

    private const Yaku HanMask4 =
      Yaku.OpenChinitsu |
      Yaku.ClosedChinitsu;

    private const long ScoringFieldHanMask1 = (long) (
      ScoringFieldYaku.MenzenTsumo |
      ScoringFieldYaku.Pinfu |
      ScoringFieldYaku.OpenTanyao |
      ScoringFieldYaku.ClosedTanyao |
      ScoringFieldYaku.Iipeikou |
      ScoringFieldYaku.JikazeTon |
      ScoringFieldYaku.JikazeShaa |
      ScoringFieldYaku.JikazeNan |
      ScoringFieldYaku.JikazePei |
      ScoringFieldYaku.BakazeTon |
      ScoringFieldYaku.BakazeShaa |
      ScoringFieldYaku.BakazeNan |
      ScoringFieldYaku.BakazePei |
      ScoringFieldYaku.Haku |
      ScoringFieldYaku.Hatsu |
      ScoringFieldYaku.Chun |
      ScoringFieldYaku.OpenChanta |
      ScoringFieldYaku.OpenIttsuu |
      ScoringFieldYaku.OpenDoujun |
      ScoringFieldYaku.Ryanpeikou |
      ScoringFieldYaku.ClosedJunchan |
      ScoringFieldYaku.ClosedHonitsu |
      ScoringFieldYaku.OpenChinitsu |
      ScoringFieldYaku.Daisangen |
      ScoringFieldYaku.Suuankou |
      ScoringFieldYaku.SuuankouTanki |
      ScoringFieldYaku.Tsuuiisou |
      ScoringFieldYaku.Ryuuiisou |
      ScoringFieldYaku.Chinroutou |
      ScoringFieldYaku.ChuurenPoutou |
      ScoringFieldYaku.JunseiChuurenPoutou |
      ScoringFieldYaku.KokushiMusou |
      ScoringFieldYaku.KokushiMusouJuusanmen |
      ScoringFieldYaku.Daisuushii |
      ScoringFieldYaku.Shousuushii |
      ScoringFieldYaku.Suukantsu);

    private const long ScoringFieldHanMask2 = (long) (
      ScoringFieldYaku.Chiitoitsu |
      ScoringFieldYaku.ClosedChanta |
      ScoringFieldYaku.ClosedIttsuu |
      ScoringFieldYaku.ClosedDoujun |
      ScoringFieldYaku.Doukou |
      ScoringFieldYaku.Sankantsu |
      ScoringFieldYaku.Toitoi |
      ScoringFieldYaku.Sanankou |
      ScoringFieldYaku.Shousangen |
      ScoringFieldYaku.Honroutou |
      ScoringFieldYaku.Ryanpeikou |
      ScoringFieldYaku.OpenJunchan |
      ScoringFieldYaku.ClosedJunchan |
      ScoringFieldYaku.OpenHonitsu |
      ScoringFieldYaku.ClosedHonitsu |
      ScoringFieldYaku.ClosedChinitsu);

    private const long ScoringFieldHanMask4 = (long) (
      ScoringFieldYaku.OpenChinitsu |
      ScoringFieldYaku.ClosedChinitsu);

    internal static int CalculateScoringField(long yaku)
    {
      if ((yaku & ScoringFieldYakumanFilter) != 0)
      {
        var yakumanBits = (int) Popcnt.X64.PopCount((ulong) yaku);
        return yakumanBits + 128;
      }

      var setBits1 = (int) Popcnt.X64.PopCount((ulong) (yaku & ScoringFieldHanMask1));
      var setBits2 = (int) Popcnt.X64.PopCount((ulong) (yaku & ScoringFieldHanMask2));
      var setBits4 = (int) Popcnt.X64.PopCount((ulong) (yaku & ScoringFieldHanMask4));
      return setBits1 + 2 * setBits2 + 4 * setBits4;
    }
  }
}