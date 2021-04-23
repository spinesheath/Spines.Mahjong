using System.Collections.Generic;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Shanten;

namespace GraphicalFrontend.GameEngine
{
  internal class Seat
  {
    public List<Tile> ConcealedTiles { get; } = new();

    public Tile? CurrentDraw { get; set; }

    public TileType SeatWind { get; set; } = TileType.Ton;

    public Tile? CurrentDiscard { get; set; }

    public bool DeclaredRiichi { get; set; }

    public UkeIreCalculator Hand { get; set; } = new();

    public List<Client.Meld> Melds { get; } = new();

    public List<Tile> Discards { get; } = new();

    public bool IgnoredRonFuriten { get; set; }

    public int Score { get; set; }

    public bool IsOya => SeatWind == TileType.Ton;
  }
}