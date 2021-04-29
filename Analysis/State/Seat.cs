using System.Collections.Generic;
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
  }
}