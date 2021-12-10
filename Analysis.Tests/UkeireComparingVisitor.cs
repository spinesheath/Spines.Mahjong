using System.Text;
using Spines.Mahjong.Analysis.B9Ukeire;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten5;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class UkeireComparingVisitor : IReplayVisitor
  {
    public int EvaluationCount { get; private set; }

    public int ErrorCount { get; private set; }

    private readonly Shanten5Calculator[] _shanten5 = new Shanten5Calculator[4];
    private readonly ProgressiveUkeire[] _ukeire = new ProgressiveUkeire[4];
    private readonly byte[][] _tileCounts = new byte[4][];

    public void EndMatch()
    {
    }

    public void Seed(TileType roundWind, int honba, int riichiSticks, int dice0, int dice1, Tile doraIndicator)
    {
      _shanten5[0] = new Shanten5Calculator();
      _shanten5[1] = new Shanten5Calculator();
      _shanten5[2] = new Shanten5Calculator();
      _shanten5[3] = new Shanten5Calculator();
      _ukeire[0] = new ProgressiveUkeire();
      _ukeire[1] = new ProgressiveUkeire();
      _ukeire[2] = new ProgressiveUkeire();
      _ukeire[3] = new ProgressiveUkeire();
      _tileCounts[0] = new byte[34];
      _tileCounts[1] = new byte[34];
      _tileCounts[2] = new byte[34];
      _tileCounts[3] = new byte[34];
    }

    public void Haipai(int seatIndex, Tile[] tiles)
    {
      _shanten5[seatIndex].Haipai(tiles);
      _ukeire[seatIndex].Haipai(tiles);
      
      foreach (var tile in tiles)
      {
        _tileCounts[seatIndex][tile.TileType.TileTypeId] += 1;
      }

      var ukeire = _ukeire[seatIndex].Ukeire();
      var ukeire5 = Shanten5Ukeire(_shanten5[seatIndex], _tileCounts[seatIndex]);

      if (ukeire != ukeire5)
      {
        var hand = CreateHandString(_tileCounts[seatIndex]);
        var actual = CreateUkeireString(ukeire);
        var expected = CreateUkeireString(ukeire5);

        ErrorCount += 1;
      }
      
      EvaluationCount += 1;
    }

    public void Draw(int seatIndex, Tile tile)
    {
      _shanten5[seatIndex].Draw(tile.TileType);
      _ukeire[seatIndex].Draw(tile.TileType);

      _tileCounts[seatIndex][tile.TileType.TileTypeId] += 1;
    }

    public void Discard(int seatIndex, Tile tile)
    {
      _shanten5[seatIndex].Discard(tile.TileType);
      _ukeire[seatIndex].Discard(tile.TileType);

      _tileCounts[seatIndex][tile.TileType.TileTypeId] -= 1;

      var ukeire = _ukeire[seatIndex].Ukeire();
      var ukeire5 = Shanten5Ukeire(_shanten5[seatIndex], _tileCounts[seatIndex]);

      if (ukeire != ukeire5)
      {
        var hand = CreateHandString(_tileCounts[seatIndex]);
        var actual = CreateUkeireString(ukeire);
        var expected = CreateUkeireString(ukeire5);

        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Chii(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _shanten5[who].Chii(handTile0, handTile1);
      _ukeire[who].Chii(handTile0, handTile1);

      _tileCounts[who][handTile0.TileType.TileTypeId] -= 1;
      _tileCounts[who][handTile1.TileType.TileTypeId] -= 1;
    }

    public void Pon(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1)
    {
      _shanten5[who].Pon(handTile0.TileType);
      _ukeire[who].Pon(handTile0.TileType);

      _tileCounts[who][handTile0.TileType.TileTypeId] -= 2;
    }

    public void Daiminkan(int who, int fromWho, Tile calledTile, Tile handTile0, Tile handTile1, Tile handTile2)
    {
      _shanten5[who].Daiminkan(handTile0.TileType);
      _ukeire[who].Daiminkan(handTile0.TileType);

      _tileCounts[who][handTile0.TileType.TileTypeId] -= 3;
    }

    public void Shouminkan(int who, int fromWho, Tile calledTile, Tile addedTile, Tile handTile0, Tile handTile1)
    {
      _shanten5[who].Shouminkan(handTile0.TileType);
      _ukeire[who].Shouminkan(handTile0.TileType);

      _tileCounts[who][addedTile.TileType.TileTypeId] -= 1;
      
      var ukeire = _ukeire[who].Ukeire();
      var ukeire5 = Shanten5Ukeire(_shanten5[who], _tileCounts[who]);

      if (ukeire != ukeire5)
      {
        var hand = CreateHandString(_tileCounts[who]);
        var actual = CreateUkeireString(ukeire);
        var expected = CreateUkeireString(ukeire5);

        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    public void Ankan(int who, TileType tileType)
    {
      _shanten5[who].Ankan(tileType);
      _ukeire[who].Ankan(tileType);

      _tileCounts[who][tileType.TileTypeId] -= 4;

      var ukeire = _ukeire[who].Ukeire();
      var ukeire5 = Shanten5Ukeire(_shanten5[who], _tileCounts[who]);

      if (ukeire != ukeire5)
      {
        var hand = CreateHandString(_tileCounts[who]);
        var actual = CreateUkeireString(ukeire);
        var expected = CreateUkeireString(ukeire5);

        ErrorCount += 1;
      }

      EvaluationCount += 1;
    }

    private static ulong Shanten5Ukeire(Shanten5Calculator c, byte[] tileCounts)
    {
      var r = 0ul;
      var baseShanten = c.Shanten();
      for (var i = 0; i < 34; i++)
      {
        if (tileCounts[i] == 4)
        {
          continue;
        }

        var tile = TileType.FromTileTypeId(i);
        c.Draw(tile);
        var newShanten = c.Shanten();
        c.Discard(tile);

        if (newShanten < baseShanten)
        {
          r |= 1ul << i;
        }
      }

      return r;
    }

    private string CreateHandString(byte[] tileCounts)
    {
      var sb = new StringBuilder();
      var tileInSuit = false;
      const string suits = "mpsz";
      for (var tileTypeId = 0; tileTypeId < 34; tileTypeId++)
      {
        if (tileCounts[tileTypeId] > 0)
        {
          sb.Append($"{tileTypeId % 9 + 1}"[0], tileCounts[tileTypeId]);
          tileInSuit = true;
        }

        if ((tileTypeId % 9 == 8 || tileTypeId == 33) && tileInSuit)
        {
          sb.Append(suits[tileTypeId / 9]);
          tileInSuit = false;
        }
      }

      return sb.ToString();
    }

    private string CreateUkeireString(ulong b36)
    {
      var b = b36;
      var sb = new StringBuilder();
      var tileTypeId = 0;
      var tileInSuit = false;
      const string suits = "mpsz";
      while (b > 0)
      {
        if ((b & 1) == 1)
        {
          sb.Append(tileTypeId % 9 + 1);
          tileInSuit = true;
        }

        if ((tileTypeId % 9 == 8 || tileTypeId == 33) && tileInSuit)
        {
          sb.Append(suits[tileTypeId / 9]);
          tileInSuit = false;
        }

        b >>= 1;
        tileTypeId += 1;
      }

      if (tileInSuit)
      {
        sb.Append(suits[tileTypeId / 9]);
      }

      return sb.ToString();
    }
  }
}