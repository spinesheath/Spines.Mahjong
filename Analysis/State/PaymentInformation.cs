using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Score;

namespace Spines.Mahjong.Analysis.State
{
  public class PaymentInformation
  {
    public PaymentInformation(int fu, int han, IEnumerable<int> scoreChanges, Yaku yaku)
    {
      Fu = fu;
      Han = han;
      ScoreChanges = scoreChanges.ToList();
      Yaku = yaku;
    }

    public int Fu { get; }

    public int Han { get; }

    public IReadOnlyList<int> ScoreChanges { get; }

    public Yaku Yaku { get; }
  }
}