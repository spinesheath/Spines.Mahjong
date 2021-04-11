using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis;

namespace GraphicalFrontend.Client
{
  internal abstract class DiscardResponse
  {
    public static DiscardResponse Pass()
    {
      return new PassStrategy();
    }

    public static DiscardResponse Daiminkan()
    {
      return new DaiminkanStrategy();
    }

    public static DiscardResponse Pon(Tile tile0, Tile tile1, Tile discardAfterCall)
    {
      return new PonStrategy(tile0, tile1, discardAfterCall);
    }

    public static DiscardResponse Chii(Tile tile0, Tile tile1, Tile discardAfterCall)
    {
      return new ChiiStrategy(tile0, tile1, discardAfterCall);
    }

    public static DiscardResponse Ron()
    {
      return new RonStrategy();
    }

    internal abstract bool CanExecute(IGameState state, DiscardActions possibleActions);

    internal abstract void Execute(IClient client);

    protected static bool HasTile(IGameState state, Tile tile)
    {
      return state.ConcealedTileIds.Contains(tile.TileId);
    }

    private class PassStrategy : DiscardResponse
    {
      internal override bool CanExecute(IGameState state, DiscardActions possibleActions)
      {
        return true;
      }

      internal override void Execute(IClient client)
      {
        client.Pass();
      }
    }

    private class DaiminkanStrategy : DiscardResponse
    {
      internal override bool CanExecute(IGameState state, DiscardActions possibleActions)
      {
        return possibleActions.HasFlag(DiscardActions.Kan);
      }

      internal override void Execute(IClient client)
      {
        client.Daiminkan();
      }
    }

    private class PonStrategy : DiscardResponse
    {
      private readonly Tile _tile0;
      private readonly Tile _tile1;
      private readonly Tile _discardAfterCall;

      public PonStrategy(Tile tile0, Tile tile1, Tile discardAfterCall)
      {
        _tile0 = tile0;
        _tile1 = tile1;
        _discardAfterCall = discardAfterCall;
      }

      internal override bool CanExecute(IGameState state, DiscardActions possibleActions)
      {
        var tileType = state.RecentDiscard.TileType;
        return possibleActions.HasFlag(DiscardActions.Pon) && 
               _tile0.TileType == tileType && 
               _tile1.TileType == tileType && 
               _discardAfterCall.TileType != tileType && // kuikae
               HasTile(state, _tile0) &&
               HasTile(state, _tile1) &&
               HasTile(state, _discardAfterCall);
      }

      internal override void Execute(IClient client)
      {
        client.Pon(_tile0, _tile1, _discardAfterCall);
      }
    }

    private class ChiiStrategy : DiscardResponse
    {
      private readonly Tile _tile0;
      private readonly Tile _tile1;
      private readonly Tile _discardAfterCall;

      public ChiiStrategy(Tile tile0, Tile tile1, Tile discardAfterCall)
      {
        _tile0 = tile0;
        _tile1 = tile1;
        _discardAfterCall = discardAfterCall;
      }

      internal override bool CanExecute(IGameState state, DiscardActions possibleActions)
      {
        var tileType = state.RecentDiscard.TileType;
        var tileTypeIds = new[] {_tile0.TileType.TileTypeId, _tile1.TileType.TileTypeId, tileType.TileTypeId}.OrderBy(x => x).ToList();
        return possibleActions.HasFlag(DiscardActions.Chii) &&
               tileTypeIds[0] + 1 == tileTypeIds[1] && 
               tileTypeIds[1] + 1 == tileTypeIds[2] &&
               !KuikaeTileTypesByCalledTileTypeId[tileType.TileTypeId].Contains(_discardAfterCall.TileType) &&
               HasTile(state, _tile0) &&
               HasTile(state, _tile1) &&
               HasTile(state, _discardAfterCall);
      }

      internal override void Execute(IClient client)
      {
        client.Chii(_tile0, _tile1, _discardAfterCall);
      }

      static ChiiStrategy()
      {
        KuikaeTileTypesByCalledTileTypeId = new TileType[34][];
        for (var i = 0; i < 34; i++)
        {
          var list = new List<TileType> {TileType.FromTileTypeId(i)};
          if (i < 27 && i % 9 > 3)
          {
            list.Add(TileType.FromTileId(i - 3));
          }
          if (i < 27 && i % 9 < 6)
          {
            list.Add(TileType.FromTileId(i + 3));
          }

          KuikaeTileTypesByCalledTileTypeId[i] = list.ToArray();
        }
      }

      private static readonly TileType[][] KuikaeTileTypesByCalledTileTypeId;
    }

    private class RonStrategy : DiscardResponse
    {
      internal override bool CanExecute(IGameState state, DiscardActions possibleActions)
      {
        return possibleActions.HasFlag(DiscardActions.Ron);
      }

      internal override void Execute(IClient client)
      {
        client.Ron();
      }
    }
  }
}