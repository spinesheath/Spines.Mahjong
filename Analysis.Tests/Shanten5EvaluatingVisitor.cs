using System.Collections.Generic;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten5;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class Shanten5EvaluatingVisitor : IReplayVisitor
  {
    public int EvaluationCount { get; private set; }

    public int ErrorCount { get; private set; }

    private readonly Shanten5Calculator[] _calculators = new Shanten5Calculator[4];

    public void EndMatch()
    {
    }

    public void Seed(TileType roundWind, int honba, int riichiSticks, int dice0, int dice1, Tile doraIndicator)
    {
      _calculators[0] = new Shanten5Calculator();
      _calculators[1] = new Shanten5Calculator();
      _calculators[2] = new Shanten5Calculator();
      _calculators[3] = new Shanten5Calculator();
    }

    public void Haipai(int seatIndex, IEnumerable<Tile> tiles)
    {
      _calculators[seatIndex].Haipai(tiles);
      var shanten = _calculators[seatIndex].Shanten();

      if (shanten > 6 || shanten < 0)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Draw(int seatIndex, Tile tile)
    {
      var before = _calculators[seatIndex].Shanten();
      _calculators[seatIndex].Draw(tile.TileType);
      var shanten = _calculators[seatIndex].Shanten();

      if (shanten > before || shanten < before - 1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Discard(int seatIndex, Tile tile)
    {
      var before = _calculators[seatIndex].Shanten();
      _calculators[seatIndex].Discard(tile.TileType);
      var shanten = _calculators[seatIndex].Shanten();

      if (shanten < before || shanten > before + 1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Chii(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _calculators[who].Chii(handTile0, handTile1);
      var shanten = _calculators[who].Shanten();

      if (shanten > 6 || shanten < -1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _calculators[who].Pon(handTile0.TileType);
      var shanten = _calculators[who].Shanten();

      if (shanten > 6 || shanten < -1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2)
    {
      _calculators[who].Daiminkan(handTile0.TileType);
      var shanten = _calculators[who].Shanten();

      if (shanten > 6 || shanten < -1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1)
    {
      var before = _calculators[who].Shanten();
      _calculators[who].Shouminkan(handTile0.TileType);
      var shanten = _calculators[who].Shanten();

      if (shanten < before || shanten > before + 1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Ankan(int who, TileType tileType)
    {
      _calculators[who].Ankan(tileType);
      var shanten = _calculators[who].Shanten();

      if (shanten > 6 || shanten < -1)
      {
        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }
  }
}