﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GraphicalFrontend.Client;

namespace GraphicalFrontend.GameEngine
{
  internal class Decider
  {
    private readonly List<IPlayer> _players;
    private readonly List<VisibleBoard> _boardViews;
    private static readonly TimeSpan DecisionTimeout = TimeSpan.FromMilliseconds(4000);

    public Decider(Board board, IEnumerable<IPlayer> players)
    {
      _players = players.ToList();
      Debug.Assert(_players.Count == 4, "Needs 4 players");
      _boardViews = Enumerable.Range(0, 4).Select(i => new VisibleBoard(board, i)).ToList();
    }
    
    public async Task<DrawResponse> OnDraw(DrawActions actions, int seatIndex)
    {
      var player = _players[seatIndex];
      var boardView = _boardViews[seatIndex];
      var tile = boardView.Watashi.CurrentDraw!;

      var decider = Task.Run(() => player.OnDraw(boardView, tile, actions));
      var r = await Task.WhenAny(decider, Task.Delay(DecisionTimeout));

      if (r.IsCompletedSuccessfully && r is Task<DrawResponse> { IsCompletedSuccessfully: true } t && t.Result.CanExecute(boardView, actions))
      {
        return t.Result;
      }

      return DrawResponse.Discard(tile);
    }

    public async Task<DiscardResponse> OnDiscard(DiscardActions actions, int seatIndex)
    {
      var player = _players[seatIndex];
      var boardView = _boardViews[seatIndex];
      var tile = boardView.CurrentDiscard!;

      var decider = Task.Run(() => player.OnDiscard(boardView, tile, boardView.ActivePlayerIndex, actions));
      var r = await Task.WhenAny(decider, Task.Delay(DecisionTimeout));

      if (r.IsCompletedSuccessfully && r is Task<DiscardResponse> { IsCompletedSuccessfully: true } t && t.Result.CanExecute(boardView, actions))
      {
        return t.Result;
      }

      return DiscardResponse.Pass();
    }
  }
}