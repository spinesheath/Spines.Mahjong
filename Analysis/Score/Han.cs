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

    private const long ScoringFieldYakumanFilter = (long) (
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
      Yaku.JunseiChuurenPoutou);

    private const Yaku HanMask1 =
      Yaku.MenzenTsumo |
      Yaku.Riichi |
      Yaku.Ippatsu |
      Yaku.Chankan |
      Yaku.RinshanKaihou |
      Yaku.Haitei |
      Yaku.Houtei |
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
      Yaku.OpenDoujun |
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
      Yaku.KokushiMusouJuusanmen |
      Yaku.Daisuushii |
      Yaku.Shousuushii |
      Yaku.Suukantsu;

    private const Yaku HanMask2 =
      Yaku.DoubleRiichi |
      Yaku.Chiitoitsu |
      Yaku.ClosedChanta |
      Yaku.ClosedIttsuu |
      Yaku.ClosedDoujun |
      Yaku.Doukou |
      Yaku.Sankantsu |
      Yaku.Toitoi |
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
      Yaku.MenzenTsumo |
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
      Yaku.OpenDoujun |
      Yaku.Ryanpeikou |
      Yaku.ClosedJunchan |
      Yaku.ClosedHonitsu |
      Yaku.OpenChinitsu |
      Yaku.Daisangen |
      Yaku.Suuankou |
      Yaku.SuuankouTanki |
      Yaku.Tsuuiisou |
      Yaku.Ryuuiisou |
      Yaku.Chinroutou |
      Yaku.ChuurenPoutou |
      Yaku.JunseiChuurenPoutou |
      Yaku.KokushiMusou |
      Yaku.KokushiMusouJuusanmen |
      Yaku.Daisuushii |
      Yaku.Shousuushii |
      Yaku.Suukantsu);

    private const long ScoringFieldHanMask2 = (long) (
      Yaku.Chiitoitsu |
      Yaku.ClosedChanta |
      Yaku.ClosedIttsuu |
      Yaku.ClosedDoujun |
      Yaku.Doukou |
      Yaku.Sankantsu |
      Yaku.Toitoi |
      Yaku.Sanankou |
      Yaku.Shousangen |
      Yaku.Honroutou |
      Yaku.Ryanpeikou |
      Yaku.OpenJunchan |
      Yaku.ClosedJunchan |
      Yaku.OpenHonitsu |
      Yaku.ClosedHonitsu |
      Yaku.ClosedChinitsu);

    private const long ScoringFieldHanMask4 = (long) (
      Yaku.OpenChinitsu |
      Yaku.ClosedChinitsu);

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