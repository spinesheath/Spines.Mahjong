namespace GraphicalFrontend.GameEngine
{
  internal static class Match
  {
    public static async void Start(Decider decider, Board board)
    {
      State state = new Start();

      while (!state.IsFinal)
      {
        state.Update(board);
        await state.Decide(board, decider);
        state = state.Advance();
      }
    }
  }
}