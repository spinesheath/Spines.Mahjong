using System;
using Spines.Mahjong.Analysis.Resources;
using Spines.Mahjong.Analysis.Score;

namespace Spines.Mahjong.Analysis.Shanten
{
  public class ProgressiveScoringData : IScoringData
  {
    static ProgressiveScoringData()
    {
      LookupHonorSum = Resource.LongLookup("Scoring", "HonorSumLookup.dat");
      LookupHonorOr = Resource.LongLookup("Scoring", "HonorOrLookup.dat");
      LookupHonorWaitShift = Resource.LongLookup("Scoring", "HonorWaitShiftLookup.dat");

      LookupSuitOr = Resource.LongLookup("Scoring", "SuitOrLookup.dat");
      LookupSuitWaitShift = Resource.LongLookup("Scoring", "SuitWaitShiftLookup.dat");

      LookupFuFootprints = Resource.Lookup("Scoring", "SuitFu.dat");
    }

    public ProgressiveScoringData()
    {
      _meldLookupValues[3] |= 4L << OffsetHonorRyuuiisou;

      BigAndToSumFilter = (0b1L << BitIndex.Toitoi) | (0b1L << BitIndex.ClosedChanta);
      SankantsuSuukantsu = 1L << (BitIndex.Sankantsu - 3);

      _baseMaskFilter = ~0L;
      _baseMask = FilterOpenYaku;

      var suitWaitShift0 = LookupSuitWaitShift[0];
      WaitShiftValues = new[] {suitWaitShift0, suitWaitShift0, suitWaitShift0, LookupHonorWaitShift[0]};

      var suitOr0 = LookupSuitOr[0];
      SuitOr = new[] {suitOr0, suitOr0, suitOr0, 0L};

      HonorOr = LookupHonorOr[0] | (_meldLookupValues[3] & ~0b1_111_111_111_111L);

      HonorSum = LookupHonorSum[0] + _meldLookupValues[3];

      Fu = 20;
    }

    public void Ankan(int suitId, int index, int base5Hash)
    {
      _baseMaskFilter &= FilterNoAnkanYaku;
      SankantsuSuukantsu <<= 1;
      ShiftedAnkanCount += 1L << (BitIndex.Sanankou - 2);
      AnyKoutsu(suitId, index, base5Hash, 16);
    }

    public void Chii(int suitId, int index, int base5Hash)
    {
      _meldLookupValues[suitId] |= 0b101000L << OffsetSuitHonitsu;

      if (index % 3 == 0)
      {
        _meldLookupValues[suitId] |= 1L << (OffsetSuitIttsuu + index / 3);
      }

      _meldLookupValues[suitId] |= (index + 9L) | (1L << 15 << index);

      _baseMask = FilterClosedYaku;
      OpenBit = 1L;

      BigAndToSumFilter = 0b1L << BitIndex.ClosedChanta;

      _baseMaskFilter &= FilterChinroutouCall & FilterHonroutouCall;

      if (suitId == 2)
      {
        if (index != 1)
        {
          _baseMaskFilter &= FilterRyuuiisou;
        }
      }
      else
      {
        _baseMaskFilter &= FilterRyuuiisou;
      }

      if (index == 0 || index == 6)
      {
        _baseMaskFilter &= FilterNoChantaCalls;
      }
      else
      {
        _baseMaskFilter &= FilterOnlyChantaCalls;
      }

      UpdateSuit(suitId, base5Hash);
    }

    public ProgressiveScoringData Clone()
    {
      var c = new ProgressiveScoringData
      {
        _baseMask = _baseMask,
        _baseMaskFilter = _baseMaskFilter,
        OpenBit = OpenBit,
        ShiftedAnkanCount = ShiftedAnkanCount,
        BigAndToSumFilter = BigAndToSumFilter,
        SankantsuSuukantsu = SankantsuSuukantsu,
        HonorSum = HonorSum,
        HonorOr = HonorOr,
        Fu = Fu
      };

      Array.Copy(_meldLookupValues, c._meldLookupValues, _meldLookupValues.Length);
      Array.Copy(WaitShiftValues, c.WaitShiftValues, WaitShiftValues.Length);
      Array.Copy(SuitOr, c.SuitOr, SuitOr.Length);
      Array.Copy(_fuFootprintOffsets, c._fuFootprintOffsets, c._fuFootprintOffsets.Length);

      return c;
    }

    public void Daiminkan(int suitId, int index, int base5Hash)
    {
      _baseMask = FilterClosedYaku;
      SankantsuSuukantsu <<= 1;
      OpenBit = 1L;
      AnyKoutsu(suitId, index, base5Hash, 8);
    }

    public void Discard(int suitId, int base5Hash)
    {
      UpdateSuit(suitId, base5Hash);
    }

    public void Draw(int suitId, int base5Hash)
    {
      UpdateSuit(suitId, base5Hash);
    }

    public void Init(int[] base5Hashes)
    {
      UpdateSuit(0, base5Hashes[0]);
      UpdateSuit(1, base5Hashes[1]);
      UpdateSuit(2, base5Hashes[2]);
      UpdateSuit(3, base5Hashes[3]);
    }

    public void Pon(int suitId, int index, int base5Hash)
    {
      _baseMask = FilterClosedYaku;
      OpenBit = 1L;
      AnyKoutsu(suitId, index, base5Hash, 2);
    }

    public void Shouminkan(TileType tileType, int base5Hash)
    {
      SankantsuSuukantsu <<= 1;

      if (tileType.IsKyuuhai)
      {
        Fu += 12; // Upgrade from pon to kan
      }
      else
      {
        Fu += 6; // Upgrade from pon to kan
      }

      UpdateSuit(tileType.SuitId, base5Hash);
    }

    public int FuFootprint(int suitId, int index)
    {
      return LookupFuFootprints[_fuFootprintOffsets[suitId] + index];
    }

    public long[] WaitShiftValues { get; }

    public long[] SuitOr { get; }

    public long HonorOr { get; private set; }

    public long HonorSum { get; private set; }

    public long FinalMask => _baseMask & _baseMaskFilter;

    public long OpenBit { get; private set; }

    public long ShiftedAnkanCount { get; private set; }

    public long BigAndToSumFilter { get; private set; }

    public long SankantsuSuukantsu { get; private set; }

    public int Fu { get; private set; }

    private const long FilterClosedYaku = ~((1L << BitIndex.ClosedSanshokuDoujun) | (1L << BitIndex.Iipeikou) |
                                            (1L << BitIndex.Chiitoitsu) | (1L << BitIndex.Ryanpeikou) |
                                            (1L << BitIndex.ClosedHonitsu) | (1L << BitIndex.ClosedChinitsu) |
                                            (1L << BitIndex.ClosedTanyao) | (1L << BitIndex.MenzenTsumo) |
                                            (1L << BitIndex.Pinfu) | (1L << BitIndex.ClosedChanta) |
                                            (1L << BitIndex.ClosedJunchan) | (1L << BitIndex.ClosedIttsuu));

    private const long FilterOpenYaku = ~((1L << BitIndex.OpenSanshokuDoujun) | (1L << BitIndex.OpenHonitsu) | (1L << BitIndex.OpenChinitsu) |
                                          (1L << BitIndex.OpenTanyao) | (1L << BitIndex.OpenChanta) | (1L << BitIndex.OpenJunchan) |
                                          (1L << BitIndex.OpenIttsuu));

    private const long FilterRyuuiisou = ~(1L << BitIndex.Ryuuiisou);

    private const long FilterHonorCall = ~((1L << BitIndex.ClosedChinitsu) | (1L << BitIndex.OpenChinitsu) |
                                           (1L << BitIndex.ClosedJunchan) | (1L << BitIndex.OpenJunchan));

    private const long FilterChinroutouCall = ~(1L << BitIndex.Chinroutou);

    private const long FilterHonroutouCall = ~(1L << BitIndex.Honroutou);

    private const long FilterNoChantaCalls = ~((1L << BitIndex.ClosedTanyao) | (1L << BitIndex.OpenTanyao));

    private const long FilterOnlyChantaCalls = ~((1L << BitIndex.ClosedChanta) | (1L << BitIndex.OpenChanta) |
                                                 (1L << BitIndex.ClosedJunchan) | (1L << BitIndex.OpenJunchan));

    private const long FilterNoAnkanYaku = ~(1L << BitIndex.Pinfu);

    private const long SetterMeldLookupForHonorKoutsu = (1L << OffsetHonorChanta) | (1L << OffsetHonorHonitsuChinitsu);
    private const long AdderMeldLookupForHonorKoutsu = 1L << OffsetHonorTsuuiisou;

    private const int OffsetSuitHonitsu = 20;
    private const int OffsetHonorTsuuiisou = BitIndex.Tsuuiisou - 3;
    private const int OffsetHonorChanta = 27;
    private const int OffsetHonorJikaze = 54;
    private const int OffsetHonorBakaze = 58;
    private const int OffsetHonorHakuHatsuChun = 15;
    private const int OffsetHonorHonitsuChinitsu = 20;
    private const int OffsetHonorRyuuiisou = 28;
    private const int OffsetHonorDaisangen = 6;
    private const int OffsetHonorShousangen = 51;
    private const int OffsetHonorShousuushii = 9;
    private const int OffsetHonorDaisuushii = 12;
    private const int OffsetSuitIttsuu = 44;

    private static readonly long[] LookupHonorSum;
    private static readonly long[] LookupHonorOr;
    private static readonly long[] LookupHonorWaitShift;
    private static readonly long[] LookupSuitOr;
    private static readonly long[] LookupSuitWaitShift;
    private static readonly byte[] LookupFuFootprints;

    private readonly int[] _fuFootprintOffsets = new int[3];

    private readonly long[] _meldLookupValues = new long[4];

    private long _baseMask;
    private long _baseMaskFilter;

    private void AnyKoutsu(int suitId, int index, int base5Hash, int baseFu)
    {
      if (suitId == 3)
      {
        _meldLookupValues[3] |= SetterMeldLookupForHonorKoutsu;
        _meldLookupValues[3] += AdderMeldLookupForHonorKoutsu;

        if (index < 4)
        {
          _meldLookupValues[3] |= ((1L << OffsetHonorJikaze) | (1L << OffsetHonorBakaze)) << index;
          _meldLookupValues[3] += (1L << OffsetHonorShousuushii) | (1L << OffsetHonorDaisuushii);
          _meldLookupValues[3] &= ~(1L << (OffsetHonorShousuushii + 2));
        }
        else
        {
          _meldLookupValues[3] |= 1L << (OffsetHonorHakuHatsuChun - 4) << index;
          _meldLookupValues[3] += (1L << OffsetHonorShousangen) | (1L << OffsetHonorDaisangen);
        }

        if (index != 5)
        {
          _meldLookupValues[3] &= ~(4L << OffsetHonorRyuuiisou);
          _baseMaskFilter &= FilterRyuuiisou;
        }

        _baseMaskFilter &= FilterHonorCall & FilterChinroutouCall & FilterNoChantaCalls;
        Fu += 2 * baseFu;
      }
      else
      {
        _meldLookupValues[suitId] |= (0b101000L << OffsetSuitHonitsu) | index | ((1L << 5) << index);

        if (index > 0 && index < 8)
        {
          _baseMaskFilter &= FilterChinroutouCall & FilterHonroutouCall & FilterOnlyChantaCalls;
          Fu += baseFu;
        }
        else
        {
          _baseMaskFilter &= FilterNoChantaCalls;
          Fu += 2 * baseFu;
        }

        if (suitId == 2)
        {
          if (index % 2 == 0 && index != 2)
          {
            _baseMaskFilter &= FilterRyuuiisou;
          }
        }
        else
        {
          _baseMaskFilter &= FilterRyuuiisou;
        }
      }

      UpdateSuit(suitId, base5Hash);
    }

    private void UpdateSuit(int suitId, int base5Hash)
    {
      if (suitId == 3)
      {
        WaitShiftValues[3] = LookupHonorWaitShift[base5Hash];
        HonorOr = LookupHonorOr[base5Hash] | (_meldLookupValues[3] & ~0b1_111_111_111_111L);
        HonorSum = LookupHonorSum[base5Hash] + _meldLookupValues[3];
      }
      else
      {
        WaitShiftValues[suitId] = LookupSuitWaitShift[base5Hash];
        var suitOr = LookupSuitOr[base5Hash] | _meldLookupValues[suitId];
        SuitOr[suitId] = suitOr + (1L << (BitIndex.ClosedIttsuu - 3));
        _fuFootprintOffsets[suitId] = (int) ((WaitShiftValues[suitId] >> 10) & 0b1111111111111L) * 680;
      }
    }
  }
}