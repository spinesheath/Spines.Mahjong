namespace Spines.Mahjong.Analysis.B9Ukeire
{
  public class ProgressiveUkeire
  {
    private readonly int[] _hashes = new int[4];
    private int _meldCount;

    public ulong Ukeire()
    {
      return new UkeireCalculator(_hashes, _meldCount).Ukeire;
    }

    public void Haipai(Tile[] tiles)
    {
      foreach (var tile in tiles)
      {
        _hashes[tile.SuitId] += tile.Base5Value;
      }
    }

    public void Draw(TileType tileType)
    {
      _hashes[tileType.SuitId] += tileType.Base5Value;
    }

    public void Discard(TileType tileType)
    {
      _hashes[tileType.SuitId] -= tileType.Base5Value;
    }

    public void Chii(Tile handTile0, Tile handTile1)
    {
      _hashes[handTile0.SuitId] -= handTile0.Base5Value;
      _hashes[handTile1.SuitId] -= handTile1.Base5Value;
      _meldCount += 1;
    }

    public void Pon(TileType tileType)
    {
      _hashes[tileType.SuitId] -= 2 * tileType.Base5Value;
      _meldCount += 1;
    }

    public void Daiminkan(TileType tileType)
    {
      _hashes[tileType.SuitId] -= 3 * tileType.Base5Value;
      _meldCount += 1;
    }

    public void Shouminkan(TileType tileType)
    {
      _hashes[tileType.SuitId] -= tileType.Base5Value;
    }

    public void Ankan(TileType tileType)
    {
      _hashes[tileType.SuitId] -= 4 * tileType.Base5Value;
      _meldCount += 1;
    }
  }
}