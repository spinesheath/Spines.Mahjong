using System;
using System.Linq;
using System.Threading;
using GraphicalFrontend.Client;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.Ai
{
  internal class SimpleAi : IPlayer
  {
    public SimpleAi(string tenhouId, string lobby, bool withDelay)
    {
      _withDelay = withDelay;
      Id = tenhouId;
      Lobby = lobby;
      _random = new Random();
    }

    public string Id { get; }

    public string Lobby { get; }

    public DrawResponse OnDraw(IGameState state, Tile tile, DrawActions suggestedActions)
    {
      if (suggestedActions.HasFlag(DrawActions.Tsumo))
      {
        Delay(500);
        return DrawResponse.Tsumo();
      }

      if (suggestedActions.HasFlag(DrawActions.Riichi))
      {
        var tileTypeId = state.Hand.GetHighestUkeIreDiscard();
        var discard = state.ConcealedTiles.First(i => i.TileType.TileTypeId == tileTypeId);
        Delay(1000);
        return DrawResponse.Riichi(discard);
      }

      if (suggestedActions.HasFlag(DrawActions.KyuushuKyuuhai) && state.Hand.Shanten > 2)
      {
        Delay(1000);
        return DrawResponse.KyuushuKyuuhai();
      }

      {
        var tileTypeId = state.Hand.GetHighestUkeIreDiscard();
        // Prefer tsumogiri
        if (tile.TileType.TileTypeId == tileTypeId)
        {
          Delay(500);
          return DrawResponse.Discard(tile);
        }

        var discard = state.ConcealedTiles.First(i => i.TileType.TileTypeId == tileTypeId);
        Delay(1000);
        return DrawResponse.Discard(discard);
      }
    }

    public DiscardResponse OnDiscard(IGameState state, Tile tile, int who, DiscardActions suggestedActions)
    {
      if (suggestedActions.HasFlag(DiscardActions.Ron))
      {
        Delay(500);
        return DiscardResponse.Ron();
      }

      // Call value honors if it improves shanten
      if (suggestedActions.HasFlag(DiscardActions.Pon) && !suggestedActions.HasFlag(DiscardActions.Kan))
      {
        var tileType = tile.TileType;
        if (tileType.TileTypeId >= 31 || tileType == state.RoundWind || tileType == state.SeatWind)
        {
          var t = state.Hand.WithPon(tileType);
          if (t.Shanten < state.Hand.Shanten)
          {
            var tilesInHand = state.ConcealedTiles.Where(i => i.TileType.TileTypeId == tileType.TileTypeId).ToList();

            var tileTypeId = t.GetHighestUkeIreDiscard();
            var discard = state.ConcealedTiles.First(i => i.TileType.TileTypeId == tileTypeId);

            Delay(1000);
            return DiscardResponse.Pon(tilesInHand[0], tilesInHand[1], discard);
          }
        }
      }

      Delay(500);
      return DiscardResponse.Pass();
    }

    public bool Chankan(IGameState state, Tile tile, int who)
    {
      Delay(500);
      return true;
    }

    private readonly Random _random;
    private readonly bool _withDelay;

    private void Delay(int max)
    {
      if (_withDelay)
      {
        Thread.Sleep(_random.Next(100, max));
      }
    }
  }
}