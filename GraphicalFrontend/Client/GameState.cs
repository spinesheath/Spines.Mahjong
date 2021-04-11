﻿using System.Collections.Generic;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;

namespace GraphicalFrontend.Client
{
  internal class GameState : IGameState
  {
    /// <summary>
    /// 0-3 for east, 4-7 for south, 8-11 for west
    /// </summary>
    public int Round { get; set; }

    public int Honba { get; set; }

    public int RiichiSticks { get; set; }

    public int Dice0 { get; set; }

    public int Dice1 { get; set; }

    IReadOnlyList<int> IGameState.DoraIndicators => DoraIndicators;

    public List<int> DoraIndicators { get; } = new();

    IUkeIreAnalysis IGameState.Hand => Hand;

    public UkeIreCalculator Hand { get; set; } = new();

    public int Oya { get; set; }

    public int Score { get; set; }

    public List<Pond> Ponds { get; set; } = new();

    public List<Opponent> Opponents { get; set; } = new()
    {
      new Opponent(),
      new Opponent(),
      new Opponent()
    };

    public bool DeclaredRiichi { get; set; }

    IReadOnlyList<int> IGameState.ConcealedTileIds => ConcealedTileIds;

    public int? RecentDraw { get; set; }

    IReadOnlyList<MeldDecoder> IGameState.Melds => Melds;

    public List<MeldDecoder> Melds { get; set; } = new();

    public List<int> ConcealedTileIds { get; set; } = new();

    public Tile? RecentDiscard { get; set; }

    public TileType SeatWind => TileType.FromTileTypeId(27 + (4 - Oya) % 4);

    public TileType RoundWind => TileType.FromTileTypeId(27 + Round / 4);
  }
}