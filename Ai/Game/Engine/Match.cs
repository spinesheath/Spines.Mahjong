using System.Threading.Tasks;
using Game.Shared;

namespace Game.Engine
{
  public static class Match
  {
    public static async Task Start(IPlayer p0, IPlayer p1, IPlayer p2, IPlayer p3, ISpectator spectator)
    {
      var wall = new Wall();
      var board = new Board(wall);
      var decider = new Decider(board, new[] { p0, p1, p2, p3 });
      await Start(decider, board, wall, spectator);
    }

    public static Task Start(IPlayer p0, IPlayer p1, IPlayer p2, IPlayer p3)
    {
      return Start(p0, p1, p2, p3, new NullSpectator());
    }

    private static async Task Start(Decider decider, Board board, Wall wall, ISpectator spectator)
    {

      State state = new Start();
      var spectatorBoard = new VisibleBoard(board);

      while (!state.IsFinal)
      {
        state.Update(board, wall);
        spectator.Updated(spectatorBoard);
        await state.Decide(board, decider);
        state = state.Advance();
      }

      spectator.Updated(spectatorBoard);
    }
  }
}