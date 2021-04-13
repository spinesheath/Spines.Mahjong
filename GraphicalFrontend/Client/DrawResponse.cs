using System.Linq;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Replay;

namespace GraphicalFrontend.Client
{
  internal abstract class DrawResponse
  {
    public static DrawResponse Discard(Tile tile)
    {
      return new DiscardStrategy(tile);
    }

    public static DrawResponse Ankan(TileType tileType)
    {
      return new AnkanStrategy(tileType);
    }

    public static DrawResponse Shouminkan(Tile tile)
    {
      return new ShouminkanStrategy(tile);
    }

    public static DrawResponse Tsumo()
    {
      return new TsumoStrategy();
    }

    public static DrawResponse Riichi(Tile tile)
    {
      return new RiichiStrategy(tile);
    }

    public static DrawResponse KyuushuKyuuhai()
    {
      return new KyuushuStrategy();
    }

    protected static bool HasTile(IGameState state, Tile tile)
    {
      return state.ConcealedTiles.Contains(tile);
    }

    internal abstract bool CanExecute(IGameState state, DrawActions possibleActions);

    internal abstract void Execute(IClient client);

    internal class DiscardStrategy : DrawResponse
    {
      private readonly Tile _tile;

      public DiscardStrategy(Tile tile)
      {
        _tile = tile;
      }

      internal override bool CanExecute(IGameState state, DrawActions possibleActions)
      {
        return HasTile(state, _tile);
      }

      internal override void Execute(IClient client)
      {
        client.Discard(_tile);
      }
    }

    internal class AnkanStrategy : DrawResponse
    {
      private readonly TileType _tileType;

      public AnkanStrategy(TileType tileType)
      {
        _tileType = tileType;
      }

      internal override bool CanExecute(IGameState state, DrawActions possibleActions)
      {
        if (!possibleActions.HasFlag(DrawActions.Kan) || state.ConcealedTiles.Count(t => t.TileType == _tileType) != 4 || state.RecentDraw == null)
        {
          return false;
        }
        
        return !state.DeclaredRiichi || !state.Hand.IsUkeIreChangedByAnkan(state.RecentDraw.TileType, _tileType);
      }

      internal override void Execute(IClient client)
      {
        client.Ankan(_tileType);
      }
    }

    internal class ShouminkanStrategy : DrawResponse
    {
      private readonly Tile _tile;

      public ShouminkanStrategy(Tile tile)
      {
        _tile = tile;
      }

      internal override bool CanExecute(IGameState state, DrawActions possibleActions)
      {
        return possibleActions.HasFlag(DrawActions.Kan) && 
               !state.DeclaredRiichi && 
               HasTile(state, _tile) && 
               state.Melds.Any(m => m.MeldType == MeldType.Koutsu && m.LowestTile.TileType == _tile.TileType);
      }

      internal override void Execute(IClient client)
      {
        client.Shouminkan(_tile);
      }
    }

    internal class TsumoStrategy : DrawResponse
    {
      internal override bool CanExecute(IGameState state, DrawActions possibleActions)
      {
        return possibleActions.HasFlag(DrawActions.Tsumo);
      }

      internal override void Execute(IClient client)
      {
        client.Tsumo();
      }
    }

    internal class RiichiStrategy : DrawResponse
    {
      private readonly Tile _tile;

      public RiichiStrategy(Tile tile)
      {
        _tile = tile;
      }

      internal override bool CanExecute(IGameState state, DrawActions possibleActions)
      {
        return possibleActions.HasFlag(DrawActions.Riichi) && 
               HasTile(state, _tile) && 
               state.Hand.ShantenAfterDiscard(_tile.TileType) == 0;
      }

      internal override void Execute(IClient client)
      {
        client.Riichi(_tile);
      }
    }

    internal class KyuushuStrategy : DrawResponse
    {
      internal override bool CanExecute(IGameState state, DrawActions possibleActions)
      {
        return possibleActions.HasFlag(DrawActions.KyuushuKyuuhai);
      }

      internal override void Execute(IClient client)
      {
        client.KyuushuKyuuhai();
      }
    }
  }
}