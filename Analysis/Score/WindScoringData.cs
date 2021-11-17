namespace Spines.Mahjong.Analysis.Score
{
  public class WindScoringData
  {
    public WindScoringData(int roundWind, int seatWind)
    {
      HonorShift = (1 << roundWind) | (1 << seatWind);

      var mask = ~((0b1111L << BitIndex.BakazeTon) | (0b1111L << BitIndex.JikazeTon));
      mask |= 0b1L << (BitIndex.BakazeTon + roundWind);
      mask |= 0b1L << (BitIndex.JikazeTon + seatWind);
      ValueWindFilter = mask;
      
      DoubleValueWindBit = roundWind == seatWind ? 1 : 0;
    }

    public int HonorShift { get; }

    public long ValueWindFilter { get; }

    public long DoubleValueWindBit { get; }
  }
}