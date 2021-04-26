using System;
using System.Threading;
using Game.Shared;
using Spines.Mahjong.Analysis;

namespace SimpleAi
{
  public class Delayed : IPlayer
  {
    private readonly Random _random;
    private readonly IPlayer _player;

    public Delayed(IPlayer player)
    {
      _player = player;
      _random = new Random();
    }

    public string Id => _player.Id;

    public string Lobby => _player.Lobby;

    public DrawResponse OnDraw(VisibleBoard board, Tile tile, DrawActions suggestedActions)
    {
      Delay(50);
      return _player.OnDraw(board, tile, suggestedActions);
    }

    public DiscardResponse OnDiscard(VisibleBoard board, Tile tile, int who, DiscardActions suggestedActions)
    {
      Delay(50);
      return _player.OnDiscard(board, tile, who, suggestedActions);
    }

    public bool Chankan(VisibleBoard board, Tile tile, int who)
    {
      Delay(50);
      return _player.Chankan(board, tile, who);
    }

    private void Delay(int max)
    {
      Thread.Sleep(_random.Next(max / 10, max));
    }
  }
}