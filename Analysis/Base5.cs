namespace Spines.Mahjong.Analysis
{
  public static class Base5
  {
    /// <summary>
    /// 10_000_000 in base5
    /// </summary>
    public const int MaxFor7Digits = 78125;

    /// <summary>
    /// 1_000_000_000 in base5
    /// </summary>
    public const int MaxFor9Digits = 1953125;
    
    /// <summary>
    /// pow(5, index)
    /// </summary>
    public static readonly int[] Table =
    {
      1,
      5,
      25,
      125,
      625,
      3125,
      15625,
      78125,
      390625
    };
  }
}
