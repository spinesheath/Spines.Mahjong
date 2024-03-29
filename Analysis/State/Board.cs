﻿using System.Collections.Generic;
using System.Linq;

namespace Spines.Mahjong.Analysis.State
{
  public class Board
  {
    public Board(IWall wall)
    {
      Wall = wall;
    }

    public Seat ActiveSeat => Seats[ActiveSeatIndex];

    public int ActiveSeatIndex { get; set; }

    public Tile? CurrentDiscard => Seats[ActiveSeatIndex].CurrentDiscard;

    public int Honba { get; set; }

    public bool IsFirstGoAround => Wall.RemainingDraws >= 66 && !Seats.SelectMany(s => s.Melds).Any();

    public Seat Oya => Seats.First(s => s.IsOya);

    public int RiichiSticks { get; set; }

    public TileType RoundWind { get; set; } = TileType.FromSuitAndIndex(Suit.Jihai, 0);

    public IReadOnlyList<Seat> Seats { get; } = new List<Seat> {new(), new(), new(), new()};

    public IWall Wall { get; }

    public void ClearCurrentDiscard()
    {
      foreach (var s in Seats)
      {
        s.CurrentDiscard = null;
      }
    }

    public void SetSeatWinds(int oya)
    {
      Seats[oya].SeatWind = TileType.Ton;
      Seats[(oya + 1) % 4].SeatWind = TileType.Nan;
      Seats[(oya + 2) % 4].SeatWind = TileType.Shaa;
      Seats[(oya + 3) % 4].SeatWind = TileType.Pei;
    }
  }
}