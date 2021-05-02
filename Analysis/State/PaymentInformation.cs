using System.Collections.Generic;

namespace Spines.Mahjong.Analysis.State
{
  public class PaymentInformation
  {
    public PaymentInformation(int fu, int han, IReadOnlyList<int> scoreChanges)
    {
      Fu = fu;
      Han = han;
      ScoreChanges = scoreChanges;
    }

    public int Fu { get; set; }

    public int Han { get; set; }

    public IReadOnlyList<int> ScoreChanges { get; }
  }
}