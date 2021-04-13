using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;

namespace GraphicalFrontend.Client
{
  internal class Meld
  {
    public Meld(MeldDecoder decoder)
    {
      MeldType = decoder.MeldType;
      Tiles = decoder.Tiles.OrderBy(t => t).Select(Tile.FromTileId).ToList();

      if (MeldType != MeldType.ClosedKan)
      {
        CalledFromPlayerOffset = decoder.CalledFromPlayerOffset;
        CalledTile = Tile.FromTileId(decoder.CalledTile);
      }

      if (MeldType == MeldType.AddedKan)
      {
        AddedTile = Tile.FromTileId(decoder.AddedTile);
      }
    }

    private Meld(IEnumerable<Tile> tiles, Tile? called, Tile? added, MeldType meldType)
    {
      Tiles = tiles.OrderBy(t => t.TileId).ToList();
      CalledTile = called;
      AddedTile = added;
      MeldType = meldType;
    }

    public Tile? AddedTile { get; }

    public int? CalledFromPlayerOffset { get; }

    public Tile? CalledTile { get; }

    public Tile LowestTile => Tiles[0];

    public MeldType MeldType { get; }

    public IReadOnlyList<Tile> Tiles { get; }

    public static Meld Ankan(TileType tileType)
    {
      var tiles = Enumerable.Range(0, 4).Select(i => Tile.FromTileType(tileType, i));
      return new(tiles, null, null, MeldType.ClosedKan);
    }

    public static Meld Chii(IEnumerable<Tile> tiles, Tile calledTile)
    {
      return new(tiles, calledTile, null, MeldType.Shuntsu);
    }

    public static Meld Daiminkan(Tile calledTile)
    {
      var tiles = Enumerable.Range(0, 4).Select(i => Tile.FromTileType(calledTile.TileType, i));
      return new(tiles, calledTile, null, MeldType.CalledKan);
    }

    public static Meld Pon(IEnumerable<Tile> tiles, Tile calledTile)
    {
      return new(tiles, calledTile, null, MeldType.Koutsu);
    }

    public static Meld Shouminkan(Tile calledTile, Tile addedTile)
    {
      var tiles = Enumerable.Range(0, 4).Select(i => Tile.FromTileType(calledTile.TileType, i));
      return new(tiles, calledTile, addedTile, MeldType.AddedKan);
    }
  }
}