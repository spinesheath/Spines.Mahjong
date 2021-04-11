namespace GraphicalFrontend.Client
{
  internal interface IPlayer
  {
    string Id { get; }

    string Lobby { get; }
    
    DrawResponse OnDraw(IGameState state, int tileId, DrawActions suggestedActions);

    DiscardResponse OnDiscard(IGameState state, int tileId, int who, DiscardActions suggestedActions);

    bool Chankan(IGameState state, int tileId, int who);
  }
}