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
    [Fact]
    public void BundlesWithClassicVisitor()
    {
      var files = BundlesFolders.SelectMany(Directory.EnumerateFiles);
      var visitor = new ClassicScoreCalculatingVisitor();
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        ReplayParser.Parse(fileStream, visitor);
      }

      Assert.Equal(0, visitor.FailureCount);
    }

    [Fact]
    public void BundlesWithVisitor()
    {
      var files = BundlesFolders.SelectMany(Directory.EnumerateFiles);
      var visitor = new ScoreCalculatingVisitor();
      foreach (var file in files)
      {
        using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
        ReplayParser.Parse(fileStream, visitor);
      }

      Assert.Equal(0, visitor.FailureCount);
    }

    [Theory]
    [InlineData("111222333m11z123M", "1z", Yaku.Sanankou | Yaku.OpenHonitsu)]
    [InlineData("123678m345p55789s", "1m", Yaku.Pinfu)]
    [InlineData("123456789m11122p", "1m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m11122p", "2m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m11122p", "3m", Yaku.ClosedIttsuu)]
    public void SingleWaitFu(string handString, string discardString, Yaku expectedYaku)
    {
      var discard = TileType.FromString(discardString);
      var sp = new ShorthandParser(handString);
      var hand = new HandCalculator(sp);
      var wind = new WindScoringData(0 , 0);

      var (yaku, fu) = ScoreCalculator.RonWithYaku(hand.ScoringData, wind, discard);

      Assert.Equal(expectedYaku, yaku);
    }

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
    [InlineData("11122266z333Z555Z", 0, 0, "1z", Yaku.Tsuuiisou)]
    [InlineData("234m23455p234789s", 0, 0, "4p", Yaku.Pinfu | Yaku.ClosedSanshokuDoujun)]
    [InlineData("66778899m678p678s", 0, 0, "6m", Yaku.Pinfu | Yaku.ClosedSanshokuDoujun | Yaku.Iipeikou)]
    public void SomeHandByRon(string handString, int roundWind, int seatWind, string discardString, Yaku expectedYaku)
    {
      var discard = TileType.FromString(discardString);
      var sp = new ShorthandParser(handString);
      var hand = new HandCalculator(sp);
      var wind = new WindScoringData(roundWind, seatWind);

      var (yaku, fu) = ScoreCalculator.RonWithYaku(hand.ScoringData, wind, discard);

      Assert.Equal(expectedYaku, yaku);
    }

    [Theory]
    [InlineData("56788m456p567s 7777P", 0, 3, "5m", Yaku.MenzenTsumo | Yaku.ClosedTanyao)]
    public void SomeHandByTsumo(string handString, int roundWind, int seatWind, string discardString, Yaku expectedYaku)
    {
      var discard = TileType.FromString(discardString);
      var sp = new ShorthandParser(handString);
      var hand = new HandCalculator(sp);
      var wind = new WindScoringData(roundWind, seatWind);

      var (yaku, fu) = ScoreCalculator.TsumoWithYaku(hand.ScoringData, wind, discard);

      Assert.Equal(expectedYaku, yaku);
    }

    [Theory]
    [InlineData("999m456p234789s44z", 0, 2, "3s", 40)]
    [InlineData("23477788m222z555M", 0, 1, "4m", 40)]
    [InlineData("23445688m456s777Z", 0, 0, "4s", 30)]
    [InlineData("345p22456s111z345M", 0, 3, "6s", 30)]
    [InlineData("111m55p678s789M333Z", 1, 2, "1m", 30)]
    [InlineData("234456p33377s555z", 1, 3, "5z", 40)]
    [InlineData("123778899p11234s", 1, 1, "1s", 40)]
    [InlineData("678m44678s 567P 234P", 0, 3, "8s", 30)]
    [InlineData("44m234p567s222777z", 0, 1, "7z", 50)]
    [InlineData("77788999s 789M 222Z", 0, 1, "9s", 40)]
    [InlineData("12333345666p 999M", 1, 2, "6p", 30)]
    [InlineData("45666678999m 333P", 0, 3, "8m", 30)]
    [InlineData("11123444m 456S 111Z", 0, 0, "1m", 30)]
    [InlineData("22266678999p 345P", 0, 2, "6p", 40)]
    [InlineData("33345678999s 777Z", 0, 1, "9s", 30)]
    [InlineData("33344455577m567s", 0, 3, "4m", 40)]
    [InlineData("234m77p111222333s", 1, 3, "3s", 40)]
    [InlineData("22233344477s 234M", 2, 1, "3s", 30)]
    [InlineData("44789p123456789s", 1, 2, "2s", 40)]
    [InlineData("111m33999s 444Z777Z", 1, 2, "1m", 40)]
    [InlineData("123p11123444s111z", 0, 1, "1z", 50)]
    [InlineData("789p111222333s44z", 0, 3, "4z", 40)]
    [InlineData("99m112233p123s222z", 0, 1, "2z", 40)]
    [InlineData("11789m778899p111z", 0, 3, "1z", 40)]
    [InlineData("234m11123444p234s", 1, 2, "1p", 40)]
    [InlineData("333m22233p234567s", 0, 2, "7s", 40)]
    [InlineData("111233445567s11z", 0, 3, "5s", 40)]
    [InlineData("345m345p22334455s", 0, 1, "2s", 40)]
    [InlineData("111m789p789s11z 789M", 0, 0, "9s", 40)]
    [InlineData("11144m111p 555M 111S", 1, 0, "1p", 40)]
    [InlineData("111222333p11z 333S", 1, 3, "1p", 40)]
    [InlineData("666777888s33z 678S", 0, 2, "6s", 40)]
    [InlineData("11m111222333p 123M", 0, 0, "1p", 30)]
    [InlineData("11777888999m 111P", 0, 0, "7m", 40)]
    [InlineData("11m111222333p 234M", 0, 0, "1p", 40)]
    [InlineData("11m222233334444p", 0, 0, "1m", 40)]
    [InlineData("111222333m66z 123M", 0, 0, "1m", 30)]
    public void TotalFuRon(string handString, int roundWind, int seatWind, string discardString, int expectedFu)
    {
      var discard = TileType.FromString(discardString);
      var sp = new ShorthandParser(handString);
      var hand = new HandCalculator(sp);
      var wind = new WindScoringData(roundWind, seatWind);

      var (han, fu) = ScoreCalculator.Ron(hand.ScoringData, wind, discard);

      Assert.Equal(expectedFu, fu);
    }

    [Theory]
    [InlineData("88m456789p123456s", 0, 1, "4s", 20)]
    [InlineData("678m11555p234s555z", 1, 0, "5p", 40)]
    [InlineData("22456m 999P 234S 222Z", 1, 1, "6m", 30)]
    [InlineData("335599m5566p88s33z", 0, 1, "3m", 25)]
    [InlineData("123m22p222678999s", 0, 0, "2s", 40)]
    [InlineData("12355m555567s 777Z", 0, 3, "3m", 40)]
    [InlineData("567p11123444555s", 1, 3, "1s", 40)]
    [InlineData("789m66678999p789s", 0, 3, "7m", 30)]
    [InlineData("222333444789p77z", 1, 0, "7p", 40)]
    [InlineData("666m789p66678999s", 0, 3, "6m", 40)]
    [InlineData("123m123p11123444s", 0, 0, "2p", 30)]
    [InlineData("66678999m 789P 789S", 1, 2, "6m", 30)]
    [InlineData("11123444999m 222P", 0, 0, "1m", 40)]
    [InlineData("11123444999m 222P", 0, 0, "2m", 40)]
    [InlineData("11123444m 444P 444S", 0, 0, "4m", 30)]
    [InlineData("456p66777888999s", 0, 0, "9s", 40)]
    [InlineData("11112233444m222p", 0, 0, "4m", 30)]
    public void TotalFuTsumo(string handString, int roundWind, int seatWind, string drawString, int expectedFu)
    {
      var draw = TileType.FromString(drawString);
      var sp = new ShorthandParser(handString);
      var hand = new HandCalculator(sp);
      var wind = new WindScoringData(roundWind, seatWind);
      
      var (han, fu) = ScoreCalculator.Tsumo(hand.ScoringData, wind, draw);

      Assert.Equal(expectedFu, fu);
    }

    [Theory]
    [InlineData("123456789m12344p44z", 0, 0, "2m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m12344p44z", 0, 1, "2m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m12344p44z", 0, 2, "2m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m12344p44z", 0, 3, "2m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m12344p44z", 1, 1, "2m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m12344p44z", 1, 2, "2m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m12344p44z", 1, 3, "2m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m12344p44z", 2, 2, "2m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m12344p44z", 2, 3, "2m", Yaku.ClosedIttsuu)]
    [InlineData("123456789m12344p44z", 3, 3, "2m", Yaku.ClosedIttsuu)]
    public void ValuePairFu(string handString, int roundWind, int seatWind, string discardString, Yaku expectedYaku)
    {
      var discard = TileType.FromString(discardString);
      var sp = new ShorthandParser(handString);
      var hand = new HandCalculator(sp);
      var wind = new WindScoringData(roundWind, seatWind);

      var (yaku, fu) = ScoreCalculator.RonWithYaku(hand.ScoringData, wind, discard);

      Assert.Equal(expectedYaku, yaku);
    }

    private static readonly string[] BundlesFolders =
    {
      @"C:\tenhou\compressed\2014\yonma\bundles",
      @"C:\tenhou\compressed\2015\yonma\bundles",
      @"C:\tenhou\compressed\2016\yonma\bundles"
    };
  }
}