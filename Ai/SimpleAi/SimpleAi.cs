using System;
using System.Linq;
using System.Threading;
using Game.Shared;
using Spines.Mahjong.Analysis;

namespace SimpleAi
{
  public class SimpleAi : IPlayer
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
    
    public DrawResponse OnDraw(VisibleBoard board, Tile tile, DrawActions suggestedActions)
    {
      if (suggestedActions.HasFlag(DrawActions.Tsumo))
      {
        Delay(500);
        return DrawResponse.Tsumo();
      }

      if (suggestedActions.HasFlag(DrawActions.Riichi))
      {
        var tileTypeId = board.Watashi.Hand.GetHighestUkeIreDiscard();
        var discard = board.Watashi.ConcealedTiles.First(i => i.TileType.TileTypeId == tileTypeId);
        Delay(1000);
        return DrawResponse.Riichi(discard);
      }

      if (suggestedActions.HasFlag(DrawActions.KyuushuKyuuhai) && board.Watashi.Hand.Shanten > 2)
      {
        Delay(1000);
        return DrawResponse.KyuushuKyuuhai();
      }

      {
        var tileTypeId = board.Watashi.Hand.GetHighestUkeIreDiscard();
        // Prefer tsumogiri
        if (tile.TileType.TileTypeId == tileTypeId)
        {
          Delay(500);
          return DrawResponse.Discard(tile);
        }

        var discard = board.Watashi.ConcealedTiles.First(i => i.TileType.TileTypeId == tileTypeId);
        Delay(1000);
        return DrawResponse.Discard(discard);
      }
    }
    
    public DiscardResponse OnDiscard(VisibleBoard board, Tile tile, int who, DiscardActions suggestedActions)
    {
      if (suggestedActions.HasFlag(DiscardActions.Ron))
      {
        Delay(500);
        return DiscardResponse.Ron();
      }

      // Call value honors if it improves shanten. But don't call if already tenpai (all discards would be furiten)
      var shanten = board.Watashi.Hand.Shanten;
      if (shanten > 0 && suggestedActions.HasFlag(DiscardActions.Pon) && !suggestedActions.HasFlag(DiscardActions.Kan))
      {
        var tileType = tile.TileType;
        if (tileType.TileTypeId >= 31 || tileType == board.RoundWind || tileType == board.Watashi.SeatWind)
        {
          var t = board.Watashi.Hand.WithPon(tileType);
          if (t.Shanten < shanten)
          {
            var tilesInHand = board.Watashi.ConcealedTiles.Where(i => i.TileType.TileTypeId == tileType.TileTypeId).ToList();

            var tileTypeId = t.GetHighestUkeIreDiscard();
            var discard = board.Watashi.ConcealedTiles.First(i => i.TileType.TileTypeId == tileTypeId);

            Delay(1000);
            return DiscardResponse.Pon(tilesInHand[0], tilesInHand[1], discard);
          }
        }
      }

      Delay(500);
      return DiscardResponse.Pass();
    }

    public bool Chankan(VisibleBoard board, Tile tile, int who)
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
        //Thread.Sleep(_random.Next(100, max));
        Thread.Sleep(_random.Next(10, max / 10));
      }
    }
  }
}