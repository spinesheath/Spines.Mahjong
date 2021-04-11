namespace GraphicalFrontend.Client
{
  internal interface ISpectator
  {
    void Sent(string message);

    void Error(string message);

    void Received(string message);

    void Updated(IGameState state);
  }
}