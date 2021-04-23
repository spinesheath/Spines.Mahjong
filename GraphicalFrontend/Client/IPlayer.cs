using GraphicalFrontend.GameEngine;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.Client
{
  internal interface IPlayer
  {
    string Id { get; }

    string Lobby { get; }
    
    DrawResponse OnDraw(IGameState state, Tile tile, DrawActions suggestedActions);

    DrawResponse OnDraw(VisibleBoard board, Tile tile, DrawActions suggestedActions);

    DiscardResponse OnDiscard(IGameState state, Tile tile, int who, DiscardActions suggestedActions);

    DiscardResponse OnDiscard(VisibleBoard board, Tile tile, int who, DiscardActions suggestedActions);

    bool Chankan(IGameState state, Tile tile, int who);

    bool Chankan(VisibleBoard board, Tile tile, int who);
  }
}