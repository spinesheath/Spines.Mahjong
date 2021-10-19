using System.Runtime.Intrinsics.X86;

namespace Spines.Mahjong.Analysis.Score
{
  public static class Han
  {
    public static int Calculate(Yaku yaku)
    {
      if ((yaku & YakumanFilter) != Yaku.None)
      {
        return int.MaxValue;
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
  }
}