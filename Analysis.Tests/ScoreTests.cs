﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.Shanten;
using Xunit;

namespace Spines.Mahjong.Analysis.Tests
{
  public class ScoreTests
  {
    [Theory]
    [InlineData("789p111222333s44z", 0, 3, "4z", Yaku.Iipeikou | Yaku.ClosedChanta)]
    [InlineData("111m111789p999s44z", 0, 3, "4z", Yaku.ClosedChanta | Yaku.Sanankou)]
    [InlineData("111m111999s44z789P", 0, 3, "4z", Yaku.OpenChanta | Yaku.Sanankou)]
    [InlineData("111222333m111s44z", 0, 0, "1s", Yaku.Toitoihou | Yaku.Sanankou)]
    [InlineData("11m99p11778899s44z", 0, 0, "9s", Yaku.Chiitoitsu)]
    [InlineData("112233m55s2222M2222S", 0, 0, "2s", Yaku.Iipeikou)]
    [InlineData("111222333m234s44z", 0, 0, "2s", Yaku.Sanankou)]
    [InlineData("111222333m234s11z", 0, 0, "1m", Yaku.Iipeikou)]
    [InlineData("112233556677m11p", 0, 0, "2m", Yaku.Ryanpeikou)]
    [InlineData("111222333m12399p", 0, 0, "2p", Yaku.ClosedJunchan | Yaku.Iipeikou)]
    [InlineData("111222333m99p123P", 0, 0, "9p", Yaku.Sanankou)]
    [InlineData("123p11s789S555Z777Z", 0, 0, "2p", Yaku.Haku | Yaku.Chun | Yaku.OpenChanta)]
    [InlineData("789m789p11789s888M", 0, 0, "7s", Yaku.OpenSanshokuDoujun)]
    [InlineData("999m99p111999s123P", 0, 0, "9p", Yaku.Sanankou | Yaku.OpenJunchan)]
    [InlineData("123456789m12344p", 0, 0, "4p", Yaku.ClosedIttsuu)]
    [InlineData("123456789m44p123P", 0, 0, "4p", Yaku.OpenIttsuu)]
    [InlineData("123456m44p123P789M", 0, 0, "4p", Yaku.OpenIttsuu)]
    [InlineData("12344p123M456M789M", 0, 0, "4p", Yaku.OpenIttsuu)]
    [InlineData("11m123M1111P3333P5555P", 0, 0, "1m", Yaku.Sankantsu | Yaku.Sanankou)]
    [InlineData("11m1111P3333P5555P7777P", 0, 0, "1m", Yaku.Suukantsu | Yaku.SuuankouTanki)]
    [InlineData("222333444s66z666S", 0, 0, "2s", Yaku.Ryuuiisou)]
    [InlineData("88s234S234S666S666Z", 0, 0, "8s", Yaku.Ryuuiisou)]
    [InlineData("33344466s678M345P", 0, 0, "3s", Yaku.OpenTanyao)]
    [InlineData("45677m123M111Z777Z", 0, 0, "7m", Yaku.JikazeTon | Yaku.BakazeTon | Yaku.Chun | Yaku.OpenHonitsu)]
    [InlineData("456678s55z666Z777Z", 0, 0, "6s", Yaku.Hatsu | Yaku.Chun | Yaku.Shousangen | Yaku.OpenHonitsu)]
    [InlineData("11222233334444m", 0, 0, "1m", Yaku.Pinfu | Yaku.Ryanpeikou | Yaku.ClosedChinitsu)]
    [InlineData("11333344445555m", 0, 0, "1m", Yaku.Ryanpeikou | Yaku.ClosedChinitsu)]
    [InlineData("11123444m111p111s", 0, 0, "1m", Yaku.Sanankou)]
    [InlineData("33345666m666p666s", 0, 0, "6m", Yaku.Sanankou | Yaku.ClosedTanyao)]
    [InlineData("11123444m111p111s", 0, 0, "2m", Yaku.Sanankou | Yaku.SanshokuDoukou)]
    [InlineData("11123444m111p111s", 0, 0, "4m", Yaku.Sanankou | Yaku.SanshokuDoukou)]
    [InlineData("33345666m666p666s", 0, 0, "3m", Yaku.Sanankou | Yaku.SanshokuDoukou | Yaku.ClosedTanyao)]
    [InlineData("11122333m111p111s", 0, 0, "1m", Yaku.SanshokuDoukou | Yaku.Toitoihou | Yaku.Sanankou)]
    [InlineData("11122233399m111p", 0, 0, "1m", Yaku.Sanankou | Yaku.Toitoihou)]
    [InlineData("11777888999m111p", 0, 0, "9m", Yaku.Sanankou | Yaku.Toitoihou)]
    [InlineData("11222m222p222s222z", 0, 0, "2z", Yaku.Sanankou | Yaku.Toitoihou | Yaku.SanshokuDoukou)]
    [InlineData("11777888999m111P", 0, 0, "7m", Yaku.Toitoihou)]
    [InlineData("11777888999m222P", 0, 0, "7m", Yaku.Toitoihou)]
    [InlineData("11m111Z222Z333Z444Z", 0, 0, "1m", Yaku.Daisuushii)]
    [InlineData("111m11333444z222Z", 0, 0, "1m", Yaku.Shousuushii)]
    [InlineData("111222333m11z123M", 0, 0, "1z", Yaku.Sanankou | Yaku.OpenHonitsu)]

    public void SomeHandByRon(string handString, int roundWind, int seatWind, string discardString, Yaku expectedYaku)
    {
      var discard = TileType.FromString(discardString);
      var sp = new ShorthandParser(handString);
      var hand = new HandCalculator(sp);

      var yaku = YakuCalculator.Ron(hand, discard, roundWind, seatWind);

      Assert.Equal(expectedYaku, yaku);
    }

    [Fact]
    public void BundlesWithVisitor()
    {
      var files = BundlesFolders.SelectMany(Directory.EnumerateFiles);
      var visitor = new ScoreCalculatingVisitor();
      //foreach (var file in files.Take(500))
      foreach (var file in files.Take(100))
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        ReplayParser.Parse(fileStream, visitor);
      }

      Assert.Equal(0, visitor.FailureCount);
    }
    
    [Fact]
    public void BundlesWithClassicVisitor()
    {
      var files = BundlesFolders.SelectMany(Directory.EnumerateFiles);
      var visitor = new ClassicScoreCalculatingVisitor();
      foreach (var file in files.Take(100))
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        ReplayParser.Parse(fileStream, visitor);
      }

      Assert.Equal(0, visitor.FailureCount);
    }

    private static readonly string[] BundlesFolders = 
    {
      @"C:\tenhou\compressed\2014\yonma\bundles",
      @"C:\tenhou\compressed\2015\yonma\bundles",
      @"C:\tenhou\compressed\2016\yonma\bundles"
    };
  }
}
