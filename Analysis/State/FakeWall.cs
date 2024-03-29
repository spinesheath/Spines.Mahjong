﻿using System.Collections.Generic;

namespace Spines.Mahjong.Analysis.State
{
  public class FakeWall : IWall
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

    public void Draw()
    {
      RemainingDraws -= 1;
    }
  }
}