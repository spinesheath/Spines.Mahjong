using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.GameEngine
{
  internal class Board
  {
    public Seat ActiveSeat => Seats[ActiveSeatIndex];

    public int ActiveSeatIndex { get; set; }

    public Tile? CurrentDiscard => Seats[ActiveSeatIndex].CurrentDiscard;

    public int Honba { get; set; }

    public bool IsFirstGoAround => Wall.RemainingDraws >= 66 && !Seats.SelectMany(s => s.Melds).Any();

    public int RiichiSticks { get; set; }

    public TileType RoundWind { get; set; } = TileType.FromSuitAndIndex(Suit.Jihai, 0);

    public IReadOnlyList<Seat> Seats { get; } = new List<Seat> {new(), new(), new(), new()};

    public Wall Wall { get; } = new();

    public Seat Oya => Seats.First(s => s.IsOya);
  }
}