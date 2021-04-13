using System.Collections.Generic;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Shanten;

namespace GraphicalFrontend.Client
{
  internal interface IGameState
  {
    int Round { get; }

    int Honba { get; }

    int RiichiSticks { get; }

    IReadOnlyList<Tile> DoraIndicators { get; }

    IUkeIreAnalysis Hand { get; }

    bool DeclaredRiichi { get; }

    int Score { get; }

    IReadOnlyList<Tile> ConcealedTiles { get; }

    Tile? RecentDraw { get; }

    IReadOnlyList<Meld> Melds { get; }

    Tile? RecentDiscard { get; }

    TileType SeatWind { get; }

    TileType RoundWind { get; }

    IReadOnlyList<Pond> Ponds { get; }
  }
}