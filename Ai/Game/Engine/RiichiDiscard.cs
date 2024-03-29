﻿using System.Linq;
using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class RiichiDiscard : State
  {
    private readonly Discard _discard;
    private State? _nextState;

    public RiichiDiscard(Tile tile)
    {
      _discard = new Discard(tile);
    }

    public override State Advance()
    {
      return _nextState!;
    }

    public override async Task Decide(Board board, Decider decider)
    {
      await _discard.Decide(board, decider);
      var nextState = _discard.Advance();

      if (nextState is Ron)
      {
        _nextState = nextState;
      }
      else if (board.Seats.All(s => s.DeclaredRiichi))
      {
        _nextState = new RiichiPayment(new Abort(), board.ActiveSeatIndex);
      }
      else
      {
        _nextState = new RiichiPayment(nextState, board.ActiveSeatIndex);
      }
    }

    public override void Update(Board board, Wall wall)
    {
      _discard.Update(board, wall);
    }
  }
}