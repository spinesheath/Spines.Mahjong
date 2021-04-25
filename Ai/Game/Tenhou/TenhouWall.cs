﻿using System.Collections.Generic;
using Game.Shared;
using Spines.Mahjong.Analysis;

namespace Game.Tenhou
{
  internal class TenhouWall : IWall
  {
    private readonly List<Tile> _doraIndicators = new();

    public int RemainingDraws { get; private set; }

    public IEnumerable<Tile> DoraIndicators => _doraIndicators;

    public void Reset()
    {
      _doraIndicators.Clear();
      RemainingDraws = 70;
    }

    public void RevealDoraIndicator(Tile tile)
    {
      _doraIndicators.Add(tile);
    }
  }
}