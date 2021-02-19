using System.Collections.Generic;

namespace AnalyzerBuilder.Combinations
{
  /// <summary>
  /// Defines the shape of a meld.
  /// </summary>
  internal class Mentsu
  {
    /// <summary>
    /// The number of consecutive tile types in the meld.
    /// </summary>
    public int Stride { get; }

    /// <summary>
    /// The number of tiles per tile type in the meld.
    /// </summary>
    public int Amount { get; }

    public static IEnumerable<Mentsu> All
    {
      get
      {
        yield return Shuntsu;
        yield return Koutsu;
        yield return Kantsu;
      }
    }

    /// <summary>
    /// 3 consecutive tiles.
    /// </summary>
    public static readonly Mentsu Shuntsu = new Mentsu(3, 1);

    /// <summary>
    /// 3 tiles of one type.
    /// </summary>
    public static readonly Mentsu Koutsu = new Mentsu(1, 3);

    /// <summary>
    /// 4 tiles of one type.
    /// </summary>
    public static readonly Mentsu Kantsu = new Mentsu(1, 4);

    /// <summary>
    /// Creates an instance of Mentsu.
    /// </summary>
    private Mentsu(int stride, int amount)
    {
      Stride = stride;
      Amount = amount;
    }
  }
}