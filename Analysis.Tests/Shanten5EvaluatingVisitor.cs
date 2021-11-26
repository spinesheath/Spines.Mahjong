using System;
using System.Collections.Generic;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten5;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class Shanten5EvaluatingVisitor : IReplayVisitor
  {
    public int EvaluationCount { get; private set; }

    public int ErrorCount { get; private set; }

    private readonly int[][] _tileCounts = 
    {
      new int[34],
      new int[34],
      new int[34],
      new int[34]
    };

    private readonly int[] _meldCounts = new int[4];

    private static readonly Shanten5Calculator Calculator = new();

    public void EndMatch()
    {
    }

    public void Seed(TileType roundWind, int honba, int riichiSticks, int dice0, int dice1, Tile doraIndicator)
    {
      Array.Clear(_tileCounts[0], 0, _tileCounts[0].Length);
      Array.Clear(_tileCounts[1], 0, _tileCounts[1].Length);
      Array.Clear(_tileCounts[2], 0, _tileCounts[2].Length);
      Array.Clear(_tileCounts[3], 0, _tileCounts[3].Length);
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
      var meldCount = _meldCounts[who] += 1;

      var tileCounts = _tileCounts[who];
      tileCounts[handTile0.TileType.TileTypeId] -= 1;
      tileCounts[handTile1.TileType.TileTypeId] -= 1;
      var shanten = Calculator.Calculate(tileCounts, meldCount);

      if (shanten > 6 || shanten < -1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      var meldCount = _meldCounts[who] += 1;

      var tileCounts = _tileCounts[who];
      tileCounts[handTile0.TileType.TileTypeId] -= 2;
      var shanten = Calculator.Calculate(tileCounts, meldCount);

      if (shanten > 6 || shanten < -1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2)
    {
      var meldCount = _meldCounts[who] += 1;

      var tileCounts = _tileCounts[who];
      tileCounts[handTile0.TileType.TileTypeId] -= 3;
      var shanten = Calculator.Calculate(tileCounts, meldCount);

      if (shanten > 6 || shanten < -1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1)
    {
      var meldCount = _meldCounts[who];
      var tileCounts = _tileCounts[who];
      var before = Calculator.Calculate(tileCounts, meldCount);
      tileCounts[addedTile.TileType.TileTypeId] -= 1;
      var shanten = Calculator.Calculate(tileCounts, meldCount);

      if (shanten < before || shanten > before + 1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Ankan(int who, TileType tileType)
    {
      var meldCount = _meldCounts[who] += 1;

      var tileCounts = _tileCounts[who];
      tileCounts[tileType.TileTypeId] -= 4;
      var shanten = Calculator.Calculate(tileCounts, meldCount);

      if (shanten > 6 || shanten < -1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }
  }
}