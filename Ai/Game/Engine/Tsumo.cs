﻿using Game.Shared;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Tsumo : State
  {
    private State? _nextState;

    public override State Advance()
    {
      return _nextState!;
    }

    public override void Update(Board board, Wall wall)
    {
      // TODO calculate score

      var scoreChanges = new int[4];
      for (var i = 0; i < 4; i++)
      {
        scoreChanges[i] = i == board.ActiveSeatIndex ? 6000 + board.RiichiSticks * 1000 + board.Honba * 300 : -2000 - board.Honba * 100;
      }

      _nextState = new Payment(new EndGame(new [] {board.ActiveSeatIndex}), new PaymentInformation(0, 0, scoreChanges, Yaku.None));
    }
  }
}