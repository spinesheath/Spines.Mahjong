using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class Shouminkan : State
  {
    private readonly Tile _tile;
    private State? _nextState;

    public Shouminkan(Tile tile)
    {
      _tile = tile;
    }

    public override State Advance()
    {
      if (_ignoredRonSeats.Any())
      {
        return new IgnoredRon(_ignoredRonSeats, _nextState!);
      }

      return _nextState!;
    }

    private readonly List<int> _ignoredRonSeats = new();

    public override async Task Decide(Board board, Decider decider)
    {
      var canChankan = new bool[4];

      var reactionTasks = new Task<bool>[4];
      for (var i = 0; i < 4; i++)
      {
        if (i == board.ActiveSeatIndex)
        {
          reactionTasks[i] = Task.FromResult(false);
          continue;
        }

        if (AgariValidation.CanChankan(board, i, _tile))
        {
          canChankan[i] = true;
          reactionTasks[i] = decider.OnChankan(i, _tile);
          continue;
        }
        
        reactionTasks[i] = Task.FromResult(false);
      }

      await Task.WhenAll(reactionTasks);

      for (var i = 0; i < 4; i++)
      {
        if (canChankan[i])
        {
          _ignoredRonSeats.Add(i);
        }
      }
      
      var ronCount = reactionTasks.Count(t => t.Result);
      if (ronCount == 3)
      {
        _nextState = new Abort();
        return;
      }

      if (ronCount > 0)
      {
        _nextState = new Ron(reactionTasks.Select((t, i) => new {t.Result, i}).Where(p => p.Result).Select(r => r.i));
        return;
      }

      _nextState = new ShouminkanDraw();
    }

    public override void Update(Board board, Wall wall)
    {
      board.ActiveSeat.Shouminkan(_tile);
    }
  }
}