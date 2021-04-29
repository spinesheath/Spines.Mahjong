using System.Collections.Generic;
using System.Linq;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.State;

namespace SimpleAi
{
  public class KanAi : IPlayer
  {
    public string Id { get; } = "Kan";

    public string Lobby { get; } = "0";

    public DrawResponse OnDraw(VisibleBoard board, Tile tile, DrawActions suggestedActions)
    {
      if ((suggestedActions & DrawActions.Tsumo) != 0)
      {
        return DrawResponse.Tsumo();
      }

      if ((suggestedActions & DrawActions.KyuushuKyuuhai) != 0)
      {
        return DrawResponse.KyuushuKyuuhai();
      }

      if ((suggestedActions & DrawActions.Kan) != 0)
      {
        var ankanTileType = board.Watashi.ConcealedTiles.GroupBy(t => t.TileType).FirstOrDefault(t => t.Count() == 4)?.Key;
        if (ankanTileType != null)
        {
          return DrawResponse.Ankan(ankanTileType);
        }

        var shouminkanTile = board.Watashi.ConcealedTiles.FirstOrDefault(t => board.Watashi.Melds.Any(m => m.MeldType == MeldType.Koutsu && m.LowestTile.TileType == t.TileType));
        if (shouminkanTile != null)
        {
          return DrawResponse.Shouminkan(tile);
        }
      }

      return DrawResponse.Discard(FindDiscard(board));
    }

    public DiscardResponse OnDiscard(VisibleBoard board, Tile tile, int who, DiscardActions suggestedActions)
    {
      if ((suggestedActions & DiscardActions.Ron) != 0)
      {
        return DiscardResponse.Ron();
      }

      if ((suggestedActions & DiscardActions.Kan) != 0)
      {
        return DiscardResponse.Daiminkan();
      }

      if ((suggestedActions & DiscardActions.Pon) != 0)
      {
        var impossibleToKan = ImpossibleToKan(board);
        if (!impossibleToKan.Contains(tile.TileType))
        {
          var tiles = board.Watashi.ConcealedTiles.Where(t => t.TileType == tile.TileType).Take(2).ToList();
          var discard = FindDiscard(board);
          return tiles.Contains(discard) ? DiscardResponse.Pass() : DiscardResponse.Pon(tiles[0], tiles[1], discard);
        }
      }

      return DiscardResponse.Pass();
    }

    private Tile FindDiscard(VisibleBoard board)
    {
      var impossibleToKan = ImpossibleToKan(board);

      var grouped = board.Watashi.ConcealedTiles.GroupBy(t => t.TileType).OrderBy(g => g.Count()).ToList();
      var toDiscard = grouped.FirstOrDefault(g => impossibleToKan.Contains(g.Key)) ?? grouped.First();
      return toDiscard.First();
    }

    private static HashSet<TileType> ImpossibleToKan(VisibleBoard board)
    {
      var visibleDiscards = board.Seats.SelectMany(s => s.Discards);
      var visibleMelded = board.Seats.Skip(1).SelectMany(s => s.Melds.SelectMany(m => m.Tiles));
      var visibleIndicators = board.DoraIndicators;
      var impossibleToKan = visibleDiscards.Concat(visibleMelded).Concat(visibleIndicators).Select(t => t.TileType).Distinct().ToHashSet();
      return impossibleToKan;
    }

    public bool Chankan(VisibleBoard board, Tile tile, int who)
    {
      return true;
    }
  }
}