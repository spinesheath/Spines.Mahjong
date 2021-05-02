using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.State
{
  public class Seat
  {
    public List<Tile> ConcealedTiles { get; } = new();

    public Tile? CurrentDraw { get; set; }

    public TileType SeatWind { get; set; } = TileType.Ton;

    public Tile? CurrentDiscard { get; set; }

    public bool DeclaredRiichi { get; set; }

    public UkeIreCalculator Hand { get; set; } = new();

    public List<Meld> Melds { get; } = new();

    public List<Tile> Discards { get; } = new();

    public bool IgnoredRonFuriten { get; set; }

    public int Score { get; set; }

    public bool IsOya => SeatWind == TileType.Ton;

    public void Init(IEnumerable<Tile> tiles)
    {
      var tileList = tiles.ToList();
      
      Hand = new UkeIreCalculator();
      Hand.Init(tileList.Select(t => t.TileType));
      ConcealedTiles.Clear();
      ConcealedTiles.AddRange(tileList);
      CurrentDraw = null;
      CurrentDiscard = null;
      DeclaredRiichi = false;
      Melds.Clear();
      Discards.Clear();
    }

    public void Ankan(TileType tileType)
    {
      CurrentDraw = null;
      Hand.Ankan(tileType);
      ConcealedTiles.RemoveAll(t => t.TileType == tileType);
      Melds.Add(Meld.Ankan(tileType));
    }

    public void Discard(Tile tile)
    {
      // TODO called tiles, riichi tile
      Hand.Discard(tile.TileType);
      ConcealedTiles.Remove(tile);
      CurrentDraw = null;
      CurrentDiscard = tile;
      Discards.Add(tile);
    }

    public void Draw(Tile tile)
    {
      Hand.Draw(tile.TileType);
      ConcealedTiles.Add(tile);
      CurrentDraw = tile;
    }

    public void Chii(Tile calledTile, Tile handTile0, Tile handTile1)
    {
      var tiles = new[] { calledTile, handTile0, handTile1 };
      var lowestTileType = tiles.Select(t => t.TileType).OrderBy(t => t.TileTypeId).First();
      Hand.Chii(lowestTileType, calledTile.TileType);
      ConcealedTiles.Remove(handTile0);
      ConcealedTiles.Remove(handTile1);
      Melds.Add(Meld.Chii(tiles, calledTile));
    }

    public void Pon(Tile calledTile, Tile handTile0, Tile handTile1)
    {
      var tiles = new[] {calledTile, handTile0, handTile1};
      Hand.Pon(calledTile.TileType);
      ConcealedTiles.Remove(handTile0);
      ConcealedTiles.Remove(handTile1);
      Melds.Add(Meld.Pon(tiles, calledTile));
    }

    public void Daiminkan(Tile calledTile)
    {
      Hand.Daiminkan(calledTile.TileType);
      ConcealedTiles.RemoveAll(t => t.TileType == calledTile.TileType);
      Melds.Add(Meld.Daiminkan(calledTile));
    }

    public void Shouminkan(Tile addedTile)
    {
      CurrentDraw = null;
      Hand.Shouminkan(addedTile.TileType);
      ConcealedTiles.Remove(addedTile);

      for (var i = 0; i < Melds.Count; i++)
      {
        var meld = Melds[i];
        if (meld.MeldType == MeldType.Koutsu && meld.LowestTile.TileType == addedTile.TileType)
        {
          Melds[i] = Meld.Shouminkan(meld.CalledTile!, addedTile);
          break;
        }
      }
    }
  }
}