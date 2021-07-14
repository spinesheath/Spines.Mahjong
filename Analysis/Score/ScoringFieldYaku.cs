using System;

namespace Spines.Mahjong.Analysis.Score
{
  [Flags]
  internal enum ScoringFieldYaku : long
  {
    None = 0,
    ClosedDoujun = 1L << BitIndex.ClosedSanshokuDoujun,
    OpenDoujun = 1L << BitIndex.OpenSanshokuDoujun,
    Doukou = 1L << BitIndex.SanshokuDoukou,

    //ClosedChanta = 1L << 23,
    //OpenChanta = 1L << 24,
    //Toitoi = 1L << 25,
    //Honroutou = 1L << 26,
    //Tsuuiisou = 1L << 27,
    //Tanyao = 1L << 28,
    //ClosedJunchan = 1L << 29,
    //OpenJunchan = 1L << 30,
    //Chinroutou = 1L << 31,
    //Chuuren = 1L << 32,
    //Ryuuiisou = 1L << 33,

    Shousangen = 1L << BitIndex.Shousangen,
    Daisangen = 1L << BitIndex.Daisangen,

    Pinfu = 1L << BitIndex.Pinfu,

    Shousuushi = 1L << BitIndex.Shousuushi,
    Daisuushi = 1L << BitIndex.Daisuushi,

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

    //Iipeikou = 1L << 45,
    //Ryanpeikou = 1L << 46,
    //Shousangen = 1L << 49,
    //Daisangen = 1L << 52,
    //Shousuushi = 1L << 55,
    //Daisuushi = 1L << 58,
    //Ittsuu = 1L << 62
  }
}
