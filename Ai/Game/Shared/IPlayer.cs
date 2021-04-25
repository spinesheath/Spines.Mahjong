using Game.Engine;
using Spines.Mahjong.Analysis;

namespace Game.Shared
{
  public interface IPlayer
  {
    string Id { get; }

    string Lobby { get; }
    
    DrawResponse OnDraw(VisibleBoard board, Tile tile, DrawActions suggestedActions);
    
    DiscardResponse OnDiscard(VisibleBoard board, Tile tile, int who, DiscardActions suggestedActions);
    
    bool Chankan(VisibleBoard board, Tile tile, int who);
  }
}