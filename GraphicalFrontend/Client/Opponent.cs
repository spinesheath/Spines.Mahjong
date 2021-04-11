using System.Collections.Generic;
using Spines.Mahjong.Analysis.Replay;

namespace GraphicalFrontend.Client
{
  internal class Opponent
  {
    public List<MeldDecoder> Melds { get; set; } = new();

    public int Score { get; set; }

    public bool DeclaredRiichi { get; set; }
  }
}