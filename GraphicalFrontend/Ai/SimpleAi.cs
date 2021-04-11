using System.Linq;
using GraphicalFrontend.Client;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.Ai
{
  internal class SimpleAi : IPlayer
  {
    public SimpleAi(string tenhouId, string lobby)
    {
      Id = tenhouId;
      Lobby = lobby;
    }

    public string Id { get; }

    public string Lobby { get; }

    public DrawResponse OnDraw(IGameState state, int tileId, DrawActions suggestedActions)
    {
      if (suggestedActions.HasFlag(DrawActions.Tsumo))
      {
        return DrawResponse.Tsumo();
      }

      if (suggestedActions.HasFlag(DrawActions.Riichi))
      {
        var tileTypeId = state.Hand.GetHighestUkeIreDiscard();
        var discard = Tile.FromTileId(state.ConcealedTileIds.First(i => i / 4 == tileTypeId));
        return DrawResponse.Riichi(discard);
      }

      if (suggestedActions.HasFlag(DrawActions.KyuushuKyuuhai) && state.Hand.Shanten > 2)
      {
        return DrawResponse.KyuushuKyuuhai();
      }

      {
        var tileTypeId = state.Hand.GetHighestUkeIreDiscard();
        // Prefer tsumogiri
        if (tileId / 4 == tileTypeId)
        {
          return DrawResponse.Discard(Tile.FromTileId(tileId));
        }

        var discard = Tile.FromTileId(state.ConcealedTileIds.First(i => i / 4 == tileTypeId));
        return DrawResponse.Discard(discard);
      }
    }

    public DiscardResponse OnDiscard(IGameState state, int tileId, int who, DiscardActions suggestedActions)
    {
      if (suggestedActions.HasFlag(DiscardActions.Ron))
      {
        return DiscardResponse.Ron();
      }
      
      // Call Dragons if it improves shanten
      if (suggestedActions.HasFlag(DiscardActions.Pon) && !suggestedActions.HasFlag(DiscardActions.Kan))
      {
        var tileType = TileType.FromTileId(tileId);
        if (tileType.TileTypeId >= 31 || tileType == state.RoundWind || tileType == state.SeatWind)
        {
          var t = state.Hand.WithPon(tileType);
          if (t.Shanten < state.Hand.Shanten)
          {
            var tilesInHand = state.ConcealedTileIds.Where(i => i / 4 == tileType.TileTypeId).Select(Tile.FromTileId).ToList();

            var tileTypeId = t.GetHighestUkeIreDiscard();
            var discardTileId = Tile.FromTileId(state.ConcealedTileIds.First(i => i / 4 == tileTypeId));

            return DiscardResponse.Pon(tilesInHand[0], tilesInHand[1], discardTileId);
          }
        }
      }

      return DiscardResponse.Pass();
    }

    public bool Chankan(IGameState state, int tileId, int who)
    {
      return true;
    }
  }
}