﻿using System;

namespace Spines.Mahjong.Analysis.Score
{
  [Flags]
  internal enum ScoringAndFieldYaku : long
  {
    None = 0,
    ClosedDoujun1 = 1L << 0,
    ClosedDoujun2 = 1L << 1,
    ClosedDoujun3 = 1L << 2,
    ClosedDoujun4 = 1L << 3,
    ClosedDoujun5 = 1L << 4,
    ClosedDoujun6 = 1L << 5,
    ClosedDoujun7 = 1L << 6,
    OpenDoujun1 = 1L << 7,
    OpenDoujun2 = 1L << 8,
    OpenDoujun3 = 1L << 9,
    OpenDoujun4 = 1L << 10,
    OpenDoujun5 = 1L << 11,
    OpenDoujun6 = 1L << 12,
    OpenDoujun7 = 1L << 13,
    Doukou1 = 1L << 14,
    Doukou2 = 1L << 15,
    Doukou3 = 1L << 16,
    Doukou4 = 1L << 17,
    Doukou5 = 1L << 18,
    Doukou6 = 1L << 19,
    Doukou7 = 1L << 20,
    Doukou8 = 1L << 21,
    Doukou9 = 1L << 22,
    ClosedChanta = 1L << 23,
    OpenChanta = 1L << 24,
    Toitoi = 1L << 25,
    Honroutou = 1L << 26,
    Tsuuiisou = 1L << 27,
    Tanyao = 1L << 28,
    ClosedJunchan = 1L << 29,
    OpenJunchan = 1L << 30,
    Chinroutou = 1L << 31,
    Chuuren = 1L << 32,
    Ryuuiisou = 1L << 33,
    JikazeTon = 1L << 34,
    JikazeNan = 1L << 35,
    JikazeShaa = 1L << 36,
    JikazePei = 1L << 37,
    BakazeTon = 1L << 38,
    BakazeNan = 1L << 39,
    BakazeShaa = 1L << 40,
    BakazePei = 1L << 41,
    Haku = 1L << 42,
    Hatsu = 1L << 43,
    Chun = 1L << 44,
    Iipeikou = 1L << 45,
    Ryanpeikou = 1L << 46,
    Shousangen = 1L << 49,
    Daisangen = 1L << 52,
    Shousuushi = 1L << 55,
    Daisuushi = 1L << 58,
    Ittsuu = 1L << 62
  }
}
