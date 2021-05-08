using Spines.Mahjong.Analysis.Resources;

namespace Spines.Mahjong.Analysis.Shanten
{
  /// <summary>
  /// Calculates shanten from arrangement values.
  /// </summary>
  internal static class ArrangementClassifier
  {
    /// <summary>
    /// Calculates the shanten + 1 of 4 arrangements.
    /// Behavior for invalid inputs is undefined.
    /// Input is invalid if there is no legal 13 or 14 tile hand that is represented by these arrangements.
    /// </summary>
    /// <param name="values">The arrangement values for the 4 suits.</param>
    /// <returns>The shanten of the hand.</returns>
    public static int Classify(int[] values)
    {
      var current = Arrangement[values[0]];
      current = Arrangement[current + values[1]];
      current = Arrangement[current + values[2]];
      current = Arrangement[current + values[3]];
      return current;
    }

    private static readonly ushort[] Arrangement = Resource.Transitions("ArrangementTransitions.txt");
  }
}