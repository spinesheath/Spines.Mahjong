using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Score
{
  public static class ScoreCalculator
  {
    public static (int, int) Chankan(ProgressiveScoringData data, WindScoringData wind, TileType winningTile)
    {
      var (yaku, fu) = data.YakuAndFu(wind, winningTile, true);
      var han = Han.CalculateScoringField(yaku);
      return (han, fu);
    }

    public static (Yaku, int) ChankanWithYaku(ProgressiveScoringData data, WindScoringData wind, TileType winningTile)
    {
      var (yaku, fu) = data.YakuAndFu(wind, winningTile, true);
      return ((Yaku) yaku, fu);
    }

    public static (int, int) Ron(ProgressiveScoringData data, WindScoringData wind, TileType winningTile)
    {
      var (yaku, fu) = data.YakuAndFu(wind, winningTile, true);
      var han = Han.CalculateScoringField(yaku);
      return (han, fu);
    }

    public static (Yaku, int) RonWithYaku(ProgressiveScoringData data, WindScoringData wind, TileType winningTile)
    {
      var (yaku, fu) = data.YakuAndFu(wind, winningTile, true);
      return ((Yaku) yaku, fu);
    }

    public static (int, int) Tsumo(ProgressiveScoringData data, WindScoringData wind, TileType winningTile)
    {
      var (yaku, fu) = data.YakuAndFu(wind, winningTile, false);
      var han = Han.CalculateScoringField(yaku);
      return (han, fu);
    }

    public static (Yaku, int) TsumoWithYaku(ProgressiveScoringData data, WindScoringData wind, TileType winningTile)
    {
      var (yaku, fu) = data.YakuAndFu(wind, winningTile, false);
      return ((Yaku) yaku, fu);
    }
  }
}