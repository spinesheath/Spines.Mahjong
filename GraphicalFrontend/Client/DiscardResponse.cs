using System.Linq;
using GraphicalFrontend.GameEngine;
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

    internal abstract bool CanExecute(VisibleBoard board, DiscardActions possibleActions);

    internal abstract void Execute(IClient client);

    protected static bool HasTile(IGameState state, Tile tile)
    {
      return state.ConcealedTiles.Contains(tile);
    }

    protected static bool HasTile(VisibleBoard board, Tile tile)
    {
      return board.Watashi.ConcealedTiles.Contains(tile);
    }

    private class PassStrategy : DiscardResponse
    {
      internal override bool CanExecute(IGameState state, DiscardActions possibleActions)
      {
        return true;
      }

      internal override bool CanExecute(VisibleBoard board, DiscardActions possibleActions)
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

      internal override bool CanExecute(VisibleBoard board, DiscardActions possibleActions)
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
        if (state.RecentDiscard == null)
        {
          return false;
        }

        var tileType = state.RecentDiscard.TileType;
        return possibleActions.HasFlag(DiscardActions.Pon) && 
               _tile0.TileType == tileType && 
               _tile1.TileType == tileType && 
               _discardAfterCall.TileType != tileType && // kuikae
               HasTile(state, _tile0) &&
               HasTile(state, _tile1) &&
               HasTile(state, _discardAfterCall);
      }

      internal override bool CanExecute(VisibleBoard board, DiscardActions possibleActions)
      {
        if (board.CurrentDiscard == null)
        {
          return false;
        }

        var tileType = board.CurrentDiscard.TileType;
        return possibleActions.HasFlag(DiscardActions.Pon) &&
               _tile0.TileType == tileType &&
               _tile1.TileType == tileType &&
               _discardAfterCall.TileType != tileType && // kuikae
               HasTile(board, _tile0) &&
               HasTile(board, _tile1) &&
               HasTile(board, _discardAfterCall);
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
        if (state.RecentDiscard == null)
        {
          return false;
        }
        
        var tileType = state.RecentDiscard.TileType;
        var tileTypeIds = new[] {_tile0.TileType.TileTypeId, _tile1.TileType.TileTypeId, tileType.TileTypeId}.OrderBy(x => x).ToList();

        var isValidDiscardForKanchanCase = tileTypeIds[1] == tileType.TileTypeId && _discardAfterCall.TileType != tileType;
        var isValidDiscard = isValidDiscardForKanchanCase || Kuikae.IsValidDiscardForNonKanchanChii(tileType, _discardAfterCall.TileType);

        return possibleActions.HasFlag(DiscardActions.Chii) &&
               tileTypeIds[0] + 1 == tileTypeIds[1] && 
               tileTypeIds[1] + 1 == tileTypeIds[2] &&
               isValidDiscard &&
               HasTile(state, _tile0) &&
               HasTile(state, _tile1) &&
               HasTile(state, _discardAfterCall);
      }

      internal override bool CanExecute(VisibleBoard board, DiscardActions possibleActions)
      {
        if (board.CurrentDiscard == null)
        {
          return false;
        }

        var tileType = board.CurrentDiscard.TileType;
        var tileTypeIds = new[] { _tile0.TileType.TileTypeId, _tile1.TileType.TileTypeId, tileType.TileTypeId }.OrderBy(x => x).ToList();

        var isValidDiscardForKanchanCase = tileTypeIds[1] == tileType.TileTypeId && _discardAfterCall.TileType != tileType;
        var isValidDiscard = isValidDiscardForKanchanCase || Kuikae.IsValidDiscardForNonKanchanChii(tileType, _discardAfterCall.TileType);

        return possibleActions.HasFlag(DiscardActions.Chii) &&
               tileTypeIds[0] + 1 == tileTypeIds[1] &&
               tileTypeIds[1] + 1 == tileTypeIds[2] &&
               isValidDiscard &&
               HasTile(board, _tile0) &&
               HasTile(board, _tile1) &&
               HasTile(board, _discardAfterCall);
      }

      internal override void Execute(IClient client)
      {
        client.Chii(_tile0, _tile1, _discardAfterCall);
      }
    }

    private class RonStrategy : DiscardResponse
    {
      internal override bool CanExecute(IGameState state, DiscardActions possibleActions)
      {
        return possibleActions.HasFlag(DiscardActions.Ron);
      }

      internal override bool CanExecute(VisibleBoard board, DiscardActions possibleActions)
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