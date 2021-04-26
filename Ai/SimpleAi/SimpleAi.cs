using System.Linq;
using Game.Shared;
using Spines.Mahjong.Analysis;

namespace SimpleAi
{
  public class SimpleAi : IPlayer
  {
    public SimpleAi(string tenhouId, string lobby)
    {
      Id = tenhouId;
      Lobby = lobby;
    }

    public string Id { get; }

    public string Lobby { get; }
    
    public DrawResponse OnDraw(VisibleBoard board, Tile tile, DrawActions suggestedActions)
    {
      if (suggestedActions.HasFlag(DrawActions.Tsumo))
      {
        return DrawResponse.Tsumo();
      }

      if (suggestedActions.HasFlag(DrawActions.Riichi))
      {
        var tileTypeId = board.Watashi.Hand.GetHighestUkeIreDiscard();
        var discard = board.Watashi.ConcealedTiles.First(i => i.TileType.TileTypeId == tileTypeId);
        return DrawResponse.Riichi(discard);
      }

      if (suggestedActions.HasFlag(DrawActions.KyuushuKyuuhai) && (board.Watashi.Hand.Shanten > 2 || board.Seats.Any(s => s.DeclaredRiichi)))
      {
        return DrawResponse.KyuushuKyuuhai();
      }

      {
        var tileTypeId = board.Watashi.Hand.GetHighestUkeIreDiscard();
        // Prefer tsumogiri
        if (tile.TileType.TileTypeId == tileTypeId)
        {
          return DrawResponse.Discard(tile);
        }

        var discard = board.Watashi.ConcealedTiles.First(i => i.TileType.TileTypeId == tileTypeId);
        return DrawResponse.Discard(discard);
      }
    }
    
    public DiscardResponse OnDiscard(VisibleBoard board, Tile tile, int who, DiscardActions suggestedActions)
    {
      if (suggestedActions.HasFlag(DiscardActions.Ron))
      {
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
            
            return DiscardResponse.Pon(tilesInHand[0], tilesInHand[1], discard);
          }
        }
      }
      
      return DiscardResponse.Pass();
    }

    public bool Chankan(VisibleBoard board, Tile tile, int who)
    {
      return true;
    }
  }
}