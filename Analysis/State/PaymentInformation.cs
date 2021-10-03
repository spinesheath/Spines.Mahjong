using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Score;

namespace Spines.Mahjong.Analysis.State
{
  public class PaymentInformation
  {
    public PaymentInformation(int fu, int score, IEnumerable<int> scoreChanges, Yaku yaku)
    {
      Fu = fu;
      Score = score;
      ScoreChanges = scoreChanges.ToList();
      Yaku = yaku;
    }

    public int Fu { get; }

    public int Score { get; }

    public IReadOnlyList<int> ScoreChanges { get; }

    public Yaku Yaku { get; }
  }
}