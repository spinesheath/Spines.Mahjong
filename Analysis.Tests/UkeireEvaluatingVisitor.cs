using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class UkeireEvaluatingVisitor : IReplayVisitor
  {
    public UkeireEvaluatingVisitor()
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

    public void Haipai(int seatIndex, Tile[] tiles)
    {
      _shantenCalculators[seatIndex].Init(tiles.Select(t => t.TileType));
      var ukeire = _shantenCalculators[seatIndex].GetUkeIreFor13();

      if (ukeire.Any(u => u < -1 || u > 4))
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Draw(int seatIndex, Tile tile)
    {
      _shantenCalculators[seatIndex].Draw(tile.TileType);
    }

    public void Discard(int seatIndex, Tile tile)
    {
      _shantenCalculators[seatIndex].Discard(tile.TileType);
      var ukeire = _shantenCalculators[seatIndex].GetUkeIreFor13();

      if (ukeire.Any(u => u < -1 || u > 4))
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
    }

    public void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _shantenCalculators[who].Pon(calledTile.TileType);
    }

    public void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2)
    {
      _shantenCalculators[who].Daiminkan(calledTile.TileType);
    }

    public void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1)
    {
      _shantenCalculators[who].Shouminkan(addedTile.TileType);
    }

    public void Ankan(int who, TileType tileType)
    {
      _shantenCalculators[who].Ankan(tileType);
    }

    private List<HandCalculator> _shantenCalculators;
  }
}