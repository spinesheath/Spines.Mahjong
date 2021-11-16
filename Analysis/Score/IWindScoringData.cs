namespace Spines.Mahjong.Analysis.Score
{
  public interface IWindScoringData
  {
    /// <summary>
    /// 1 iff seat and round wind are the same
    /// </summary>
    long DoubleValueWindBit { get; }

    /// <summary>
    /// Set bits on the indexes of the value winds. Bit 0 is east, bit 3 is north.
    /// </summary>
    int HonorShift { get; }

    /// <summary>
    /// All bits set except for those corresponding to wind yakuhai that are not active with the current winds.
    /// </summary>
    long ValueWindFilter { get; }
  }
}