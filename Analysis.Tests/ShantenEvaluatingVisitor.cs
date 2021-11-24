using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class ShantenEvaluatingVisitor : IReplayVisitor
  {
    public ShantenEvaluatingVisitor()
    {
      _shantenCalculators = new List<HandCalculator>();
    }

    public int EvaluationCount { get; private set; }

    public int ErrorCount { get; private set; }

    public void EndMatch()
    {
      _shantenCalculators = new List<HandCalculator>();
    }

    public void Seed(TileType roundWind, int honba, int riichiSticks, int dice0, int dice1, Tile doraIndicator)
    {
      _shantenCalculators = new List<HandCalculator>();
      for (var i = 0; i < 4; i++)
      {
        _shantenCalculators.Add(new HandCalculator());
      }
    }

    public void Haipai(int seatIndex, IEnumerable<Tile> tiles)
    {
      _shantenCalculators[seatIndex].Init(tiles.Select(t => t.TileType));
      var shanten = _shantenCalculators[seatIndex].Shanten;
      
      if (shanten > 6 || shanten < 0)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Draw(int seatIndex, Tile tile)
    {
      var before = _shantenCalculators[seatIndex].Shanten;
      _shantenCalculators[seatIndex].Draw(tile.TileType);
      var shanten = _shantenCalculators[seatIndex].Shanten;

      if (shanten > before || shanten < before - 1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Discard(int seatIndex, Tile tile)
    {
      var before = _shantenCalculators[seatIndex].Shanten;
      _shantenCalculators[seatIndex].Discard(tile.TileType);
      var shanten = _shantenCalculators[seatIndex].Shanten;
      
      if (shanten < before || shanten > before + 1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Chii(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      var lowest = calledTile;
      if (lowest.TileId > handTile0.TileId)
      {
        lowest = handTile0;
      }

      if (lowest.TileId > handTile1.TileId)
      {
        lowest = handTile1;
      }
      
      _shantenCalculators[who].Chii(lowest.TileType, calledTile.TileType);
      var shanten = _shantenCalculators[who].Shanten;

      if (shanten > 6 || shanten < -1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _shantenCalculators[who].Pon(calledTile.TileType);
      var shanten = _shantenCalculators[who].Shanten;

      if (shanten > 6 || shanten < -1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2)
    {
      _shantenCalculators[who].Daiminkan(calledTile.TileType);
      var shanten = _shantenCalculators[who].Shanten;

      if (shanten > 6 || shanten < 0)
      {
        ErrorCount += 1;
      }
    }

    public void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1)
    {
      _shantenCalculators[who].Shouminkan(addedTile.TileType);
      var shanten = _shantenCalculators[who].Shanten;

      if (shanten > 6 || shanten < 0)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Ankan(int who, TileType tileType)
    {
      _shantenCalculators[who].Ankan(tileType);
      var shanten = _shantenCalculators[who].Shanten;

      if (shanten > 6 || shanten < 0)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    private List<HandCalculator> _shantenCalculators;
  }
}