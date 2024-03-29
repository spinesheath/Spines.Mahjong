﻿using System;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class DaiminkanDraw : DrawBase
  {
    public override void Ankan(TileType tileType)
    {
      NextState = new DoraIndicator(new Ankan(tileType));
    }

    public override void Discard(Tile tile)
    {
      // TODO document in board that there is a pending dora flip - relevant information for calling a tile
      NextState = new DoraIndicator(new Discard(tile));
    }

    public override void KyuushuKyuuhai()
    {
      throw new InvalidOperationException();
    }

    public override void Riichi(Tile tile)
    {
      throw new InvalidOperationException();
    }

    public override void Shouminkan(Tile tile)
    {
      // TODO apparently daiminkan > shouminkan > chankan means 0 dora indicators
      // TODO otherwise daiminkan > shouminkan > no chankan > dora > discard > dora
      NextState = new DoraIndicator(new Shouminkan(tile));
    }

    public override void Update(Board board, Wall wall)
    {
      var seat = board.ActiveSeat;
      seat.Draw(wall.DrawFromDeadWall());
      seat.IgnoredRonFuriten = false;
    }

    protected override DrawActions GetPossibleActions(Board board)
    {
      var suggestedActions = DrawActions.Discard;
      suggestedActions |= CanTsumo(board) ? DrawActions.Tsumo : DrawActions.Discard;
      suggestedActions |= CanKan(board) ? DrawActions.Kan : DrawActions.Discard;
      return suggestedActions;
    }

    private static bool CanTsumo(Board board)
    {
      return AgariValidation.CanTsumo(board, true);
    }
  }
}