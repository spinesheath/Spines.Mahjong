using GraphicalFrontend.Client;

namespace GraphicalFrontend.GameEngine
{
  internal static class Match
  {
    public static async void Start(Decider decider, Board board, ISpectator spectator)
    {
      State state = new Start();
      var spectatorBoard = new VisibleBoard(board);

      while (!state.IsFinal)
      {
        state.Update(board);
        spectator.Updated(spectatorBoard);
        await state.Decide(board, decider);
        state = state.Advance();
      }
    }
  }
}