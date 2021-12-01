using System.Collections.Generic;
using Spines.Mahjong.Analysis.Score;

namespace Spines.Mahjong.Analysis.State
{
  public class PaymentInformation
  {
    public PaymentInformation(int fu, int score, int[] scoreChanges, Yaku yaku)
    {
      Fu = fu;
      Score = score;
      _scoreChanges[0] = scoreChanges[0];
      _scoreChanges[1] = scoreChanges[1];
      _scoreChanges[2] = scoreChanges[2];
      _scoreChanges[3] = scoreChanges[3];
      Yaku = yaku;
    }

    public int Fu { get; }

    public int Score { get; }

    public IReadOnlyList<int> ScoreChanges => _scoreChanges;

    public Yaku Yaku { get; }

    private readonly int[] _scoreChanges = new int[4];
  }
}