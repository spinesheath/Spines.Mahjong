using System.Collections.Generic;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Shanten;

namespace GraphicalFrontend.Client
{
  internal interface IGameState
  {
    int Round { get; }

    int Honba { get; }

    int RiichiSticks { get; }

    IReadOnlyList<int> DoraIndicators { get; }

    IUkeIreAnalysis Hand { get; }

    bool DeclaredRiichi { get; }

    IReadOnlyList<int> ConcealedTileIds { get; }

    int? RecentDraw { get; }

    IReadOnlyList<MeldDecoder> Melds { get; }

    Tile? RecentDiscard { get; }

    TileType SeatWind { get; }

    TileType RoundWind { get; }
  }
}