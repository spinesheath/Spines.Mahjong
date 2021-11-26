using System.Collections.Generic;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten5;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class Shanten5EvaluatingVisitor : IReplayVisitor
  {
    public int EvaluationCount { get; private set; }

    public int ErrorCount { get; private set; }

    private readonly int[][] _tileCounts = new int[4][];

    private readonly int[] _meldCounts = new int[4];

    private static readonly Shanten5Calculator Calculator = new();

    public void EndMatch()
    {
    }

    public void Seed(TileType roundWind, int honba, int riichiSticks, int dice0, int dice1, Tile doraIndicator)
    {
      _tileCounts[0] = new int[34];
      _tileCounts[1] = new int[34];
      _tileCounts[2] = new int[34];
      _tileCounts[3] = new int[34];
      _meldCounts[0] = 0;
      _meldCounts[1] = 0;
      _meldCounts[2] = 0;
      _meldCounts[3] = 0;
    }

    public void Haipai(int seatIndex, IEnumerable<Tile> tiles)
    {
      var tileCounts = _tileCounts[seatIndex];
      foreach (var tile in tiles)
      {
        tileCounts[tile.TileType.TileTypeId] += 1;
      }
      
      var shanten = Calculator.Calculate(tileCounts, _meldCounts[seatIndex]);

      if (shanten > 6 || shanten < 0)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Draw(int seatIndex, Tile tile)
    {
      var meldCount = _meldCounts[seatIndex];
      if (meldCount > 0)
      {
        return;
      }

      var tileCounts = _tileCounts[seatIndex];
      var before = Calculator.Calculate(tileCounts, meldCount);
      tileCounts[tile.TileType.TileTypeId] += 1;
      var shanten = Calculator.Calculate(tileCounts, meldCount);

      if (shanten > before || shanten < before - 1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Discard(int seatIndex, Tile tile)
    {
      var meldCount = _meldCounts[seatIndex];
      if (meldCount > 0)
      {
        return;
      }

      var tileCounts = _tileCounts[seatIndex];
      var before = Calculator.Calculate(tileCounts, meldCount);
      tileCounts[tile.TileType.TileTypeId] -= 1;
      var shanten = Calculator.Calculate(tileCounts, meldCount);

      if (shanten < before || shanten > before + 1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Chii(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _meldCounts[who] += 1;
    }

    public void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _meldCounts[who] += 1;
    }

    public void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2)
    {
      _meldCounts[who] += 1;
    }

    public void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1)
    {
    }

    public void Ankan(int who, TileType tileType)
    {
      _meldCounts[who] += 1;
    }
  }
}