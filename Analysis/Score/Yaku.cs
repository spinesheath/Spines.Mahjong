﻿using System;

namespace Spines.Mahjong.Analysis.Score
{
  [Flags]
  public enum Yaku : long
  {
    None = 0,
    MenzenTsumo = 1L << BitIndex.MenzenTsumo,
    ClosedDoujun = 1L << BitIndex.ClosedSanshokuDoujun,
    OpenDoujun = 1L << BitIndex.OpenSanshokuDoujun,
    Doukou = 1L << BitIndex.SanshokuDoukou,
    Toitoi = 1L << BitIndex.Toitoi,
    ClosedTanyao = 1L << BitIndex.ClosedTanyao,
    OpenTanyao = 1L << BitIndex.OpenTanyao,
    Shousangen = 1L << BitIndex.Shousangen,
    Daisangen = 1L << BitIndex.Daisangen,
    Pinfu = 1L << BitIndex.Pinfu,
    Shousuushii = 1L << BitIndex.Shousuushi,
    Daisuushii = 1L << BitIndex.Daisuushi,
    JikazeTon = 1L << BitIndex.JikazeTon,
    JikazeNan = 1L << BitIndex.JikazeNan,
    JikazeShaa = 1L << BitIndex.JikazeShaa,
    JikazePei = 1L << BitIndex.JikazePei,
    BakazeTon = 1L << BitIndex.BakazeTon,
    BakazeNan = 1L << BitIndex.BakazeNan,
    BakazeShaa = 1L << BitIndex.BakazeShaa,
    BakazePei = 1L << BitIndex.BakazePei,
    Haku = 1L << BitIndex.Haku,
    Hatsu = 1L << BitIndex.Hatsu,
    Chun = 1L << BitIndex.Chun,
    Iipeikou = 1L << BitIndex.Iipeikou,
    Chiitoitsu = 1L << BitIndex.Chiitoitsu,
    Ryanpeikou = 1L << BitIndex.Ryanpeikou,
    ClosedHonitsu = 1L << BitIndex.ClosedHonitsu,
    ClosedChinitsu = 1L << BitIndex.ClosedChinitsu,
    OpenHonitsu = 1L << BitIndex.OpenHonitsu,
    OpenChinitsu = 1L << BitIndex.OpenChinitsu,
    Sanankou = 1L << BitIndex.Sanankou,
    Suuankou = 1L << BitIndex.Suuankou,
    SuuankouTanki = 1L << BitIndex.SuuankouTanki,
    Ryuuiisou = 1L << BitIndex.Ryuuiisou,
    KokushiMusou = 1L << BitIndex.KokushiMusou,
    KokushiMusouJuusanmen = 1L << BitIndex.KokushiMusouJuusanmen,
    Tsuuiisou = 1L << BitIndex.Tsuuiisou,
    Chinroutou = 1L << BitIndex.Chinroutou,
    ChuurenPoutou = 1L << BitIndex.ChuurenPoutou,
    JunseiChuurenPoutou = 1L << BitIndex.JunseiChuurenPoutou,
    ClosedChanta = 1L << BitIndex.ClosedChanta,
    OpenChanta = 1L << BitIndex.OpenChanta,
    Honroutou = 1L << BitIndex.Honroutou,
    ClosedJunchan = 1L << BitIndex.ClosedJunchan,
    OpenJunchan = 1L << BitIndex.OpenJunchan,
    ClosedIttsuu = 1L << BitIndex.ClosedIttsuu,
    OpenIttsuu = 1L << BitIndex.OpenIttsuu,
    Sankantsu = 1L << BitIndex.Sankantsu,
    Suukantsu = 1L << BitIndex.Suukantsu,

    Riichi = 1L << BitIndex.Riichi,
    DoubleRiichi = 1L << BitIndex.DoubleRiichi,
    Tenhou = 1L << BitIndex.Tenhou,
    Renhou = 1L << BitIndex.Renhou,
    Ippatsu = 1L << BitIndex.Ippatsu,
    Haitei = 1L << BitIndex.Haitei,
    Houtei = 1L << BitIndex.Houtei,
    Chankan = 1L << BitIndex.Chankan,
    RinshanKaihou = 1L << BitIndex.RinshanKaihou,
    Chiihou = 1L << BitIndex.Chiihou,

    Dora = 1L << BitIndex.Dora,
    AkaDora = 1L << BitIndex.AkaDora,
    UraDora = 1L << BitIndex.UraDora
  }
}
