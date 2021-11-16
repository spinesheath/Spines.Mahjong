using System;
using Spines.Mahjong.Analysis.Resources;
using Spines.Mahjong.Analysis.Score;

namespace Spines.Mahjong.Analysis.Shanten
{
  public class ProgressiveScoringData : IScoringData
  {
    static ProgressiveScoringData()
    {
      HonorSumLookup = Resource.LongLookup("Scoring", "HonorSumLookup.dat");
      HonorOrLookup = Resource.LongLookup("Scoring", "HonorOrLookup.dat");
      HonorWaitShiftLookup = Resource.LongLookup("Scoring", "HonorWaitShiftLookup.dat");

      SuitOrLookup = Resource.LongLookup("Scoring", "SuitOrLookup.dat");
      SuitWaitShiftLookup = Resource.LongLookup("Scoring", "SuitWaitShiftLookup.dat");

      FuFootprintsLookup = Resource.Lookup("Scoring", "SuitFu.dat");
    }

    public ProgressiveScoringData()
    {
      _meldLookupValues[3] |= 4L << HonorRyuuiisouOffset;

      BigAndToSumFilter = (0b1L << BitIndex.Toitoi) | (0b1L << BitIndex.ClosedChanta);
      SankantsuSuukantsu = 1L << (BitIndex.Sankantsu - 3);

      _baseMaskFilter = ~0L;
      _baseMask = OpenYakuFilter;
      
      var suitWaitShift0 = SuitWaitShiftLookup[0];
      WaitShiftValues = new[] {suitWaitShift0, suitWaitShift0, suitWaitShift0, HonorWaitShiftLookup[0]};
      
      var suitOr0 = SuitOrLookup[0];
      SuitOr = new[] {suitOr0, suitOr0, suitOr0, 0L};
      
      HonorOr = HonorOrLookup[0] | (_meldLookupValues[3] & ~0b1_111_111_111_111L);

      HonorSum = HonorSumLookup[0] + _meldLookupValues[3];

      Fu = 20;
    }

    public void Ankan(int suitId, int index, int base5Hash)
    {
      _baseMaskFilter &= NoAnkanYakuFilter;
      ShiftedAnkanCount += 1L << (BitIndex.Sanankou - 2);
      SankantsuSuukantsu <<= 1;

      if (suitId == 3)
      {
        _meldLookupValues[3] |= 1L << HonorChantaOffset;
        _meldLookupValues[3] |= 1L << HonorHonitsuChinitsuOffset;
        _meldLookupValues[3] += 1L << HonorTsuuiisouOffset;

        if (index < 4)
        {
          _meldLookupValues[3] |= 1L << (HonorJikazeOffset + index);
          _meldLookupValues[3] |= 1L << (HonorBakazeOffset + index);
          _meldLookupValues[3] += 1L << HonorShousuushiiOffset;
          _meldLookupValues[3] += 1L << HonorDaisuushiiOffset;
          _meldLookupValues[3] &= ~(1L << (HonorShousuushiiOffset + 2));
        }
        else
        {
          _meldLookupValues[3] |= 1L << (HonorHakuHatsuChunOffset + index - 4);
          _meldLookupValues[3] += 1L << HonorShousangenOffset;
          _meldLookupValues[3] += 1L << HonorDaisangenOffset;
        }

        if (index != 5)
        {
          _meldLookupValues[3] &= ~(4L << HonorRyuuiisouOffset);
          _baseMaskFilter &= RyuuiisouFilter;
        }

        _baseMaskFilter &= HonorCallFilter;
        _baseMaskFilter &= ChinroutouCallFilter;
        _baseMaskFilter &= NoChantaCallsFilter;
        Fu += 32;
      }
      else
      {
        _meldLookupValues[suitId] |= 0b101000L << SuitHonitsuOffset;
        _meldLookupValues[suitId] |= index + 0L;
        _meldLookupValues[suitId] |= 1L << (index + 5);

        if (index > 0 && index < 8)
        {
          _baseMaskFilter &= ChinroutouCallFilter;
          _baseMaskFilter &= HonroutouCallFilter;

          _baseMaskFilter &= OnlyChantaCallsFilter;
          Fu += 16;
        }
        else
        {
          _baseMaskFilter &= NoChantaCallsFilter;
          Fu += 32;
        }

        if (suitId == 2)
        {
          if (index % 2 == 0 && index != 2)
          {
            _baseMaskFilter &= RyuuiisouFilter;
          }
        }
        else
        {
          _baseMaskFilter &= RyuuiisouFilter;
        }
      }

      UpdateSuit(suitId, base5Hash);
    }

    public void Chii(int suitId, int index, int base5Hash)
    {
      _meldLookupValues[suitId] |= 0b101000L << SuitHonitsuOffset;

      if (index % 3 == 0)
      {
        _meldLookupValues[suitId] |= 1L << (SuitIttsuuOffset + index / 3);
      }

      _meldLookupValues[suitId] |= index + 9L;
      _meldLookupValues[suitId] |= 1L << (index + 15);

      _baseMask = ClosedYakuFilter;
      OpenBit = 1L;

      BigAndToSumFilter = 0b1L << BitIndex.ClosedChanta;

      _baseMaskFilter &= ChinroutouCallFilter;
      _baseMaskFilter &= HonroutouCallFilter;

      if (suitId == 2)
      {
        if (index != 1)
        {
          _baseMaskFilter &= RyuuiisouFilter;
        }
      }
      else
      {
        _baseMaskFilter &= RyuuiisouFilter;
      }

      if (index == 0 || index == 6)
      {
        _baseMaskFilter &= NoChantaCallsFilter;
      }
      else
      {
        _baseMaskFilter &= OnlyChantaCallsFilter;
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
      _baseMask = ClosedYakuFilter;
      OpenBit = 1L;
      SankantsuSuukantsu <<= 1;

      if (suitId == 3)
      {
        _meldLookupValues[3] |= 1L << HonorChantaOffset;
        _meldLookupValues[3] |= 1L << HonorHonitsuChinitsuOffset;
        _meldLookupValues[3] += 1L << HonorTsuuiisouOffset;

        if (index < 4)
        {
          _meldLookupValues[3] |= 1L << (HonorJikazeOffset + index);
          _meldLookupValues[3] |= 1L << (HonorBakazeOffset + index);
          _meldLookupValues[3] += 1L << HonorShousuushiiOffset;
          _meldLookupValues[3] += 1L << HonorDaisuushiiOffset;
          _meldLookupValues[3] &= ~(1L << (HonorShousuushiiOffset + 2));
        }
        else
        {
          _meldLookupValues[3] |= 1L << (HonorHakuHatsuChunOffset + index - 4);
          _meldLookupValues[3] += 1L << HonorShousangenOffset;
          _meldLookupValues[3] += 1L << HonorDaisangenOffset;
        }

        if (index != 5)
        {
          _meldLookupValues[3] &= ~(4L << HonorRyuuiisouOffset);
          _baseMaskFilter &= RyuuiisouFilter;
        }

        _baseMaskFilter &= HonorCallFilter;
        _baseMaskFilter &= ChinroutouCallFilter;
        _baseMaskFilter &= NoChantaCallsFilter;
        Fu += 16;
      }
      else
      {
        _meldLookupValues[suitId] |= 0b101000L << SuitHonitsuOffset;
        _meldLookupValues[suitId] |= index + 0L;
        _meldLookupValues[suitId] |= 1L << (index + 5);

        if (index > 0 && index < 8)
        {
          _baseMaskFilter &= ChinroutouCallFilter;
          _baseMaskFilter &= HonroutouCallFilter;
          _baseMaskFilter &= OnlyChantaCallsFilter;
          Fu += 8;
        }
        else
        {
          _baseMaskFilter &= NoChantaCallsFilter;
          Fu += 16;
        }

        if (suitId == 2)
        {
          if (index % 2 == 0 && index != 2)
          {
            _baseMaskFilter &= RyuuiisouFilter;
          }
        }
        else
        {
          _baseMaskFilter &= RyuuiisouFilter;
        }
      }

      UpdateSuit(suitId, base5Hash);
    }

    public void Discard(int suitId, int base5Hash)
    {
      UpdateSuit(suitId, base5Hash);
    }

    public void Draw(int suitId, int base5Hash)
    {
      UpdateSuit(suitId, base5Hash);
    }

    public void Pon(int suitId, int index, int base5Hash)
    {
      _baseMask = ClosedYakuFilter;
      OpenBit = 1L;

      if (suitId == 3)
      {
        _meldLookupValues[3] |= 1L << HonorChantaOffset;
        _meldLookupValues[3] |= 1L << HonorHonitsuChinitsuOffset;
        _meldLookupValues[3] += 1L << HonorTsuuiisouOffset;

        if (index < 4)
        {
          _meldLookupValues[3] |= 1L << (HonorJikazeOffset + index);
          _meldLookupValues[3] |= 1L << (HonorBakazeOffset + index);
          _meldLookupValues[3] += 1L << HonorShousuushiiOffset;
          _meldLookupValues[3] += 1L << HonorDaisuushiiOffset;
          _meldLookupValues[3] &= ~(1L << (HonorShousuushiiOffset + 2));
        }
        else
        {
          _meldLookupValues[3] |= 1L << (HonorHakuHatsuChunOffset + index - 4);
          _meldLookupValues[3] += 1L << HonorShousangenOffset;
          _meldLookupValues[3] += 1L << HonorDaisangenOffset;
        }

        if (index != 5)
        {
          _meldLookupValues[3] &= ~(4L << HonorRyuuiisouOffset);
          _baseMaskFilter &= RyuuiisouFilter;
        }

        _baseMaskFilter &= HonorCallFilter;
        _baseMaskFilter &= ChinroutouCallFilter;
        _baseMaskFilter &= NoChantaCallsFilter;
        Fu += 4;
      }
      else
      {
        _meldLookupValues[suitId] |= 0b101000L << SuitHonitsuOffset;
        _meldLookupValues[suitId] |= index + 0L;
        _meldLookupValues[suitId] |= 1L << (index + 5);

        if (index > 0 && index < 8)
        {
          _baseMaskFilter &= ChinroutouCallFilter;
          _baseMaskFilter &= HonroutouCallFilter;

          _baseMaskFilter &= OnlyChantaCallsFilter;
          Fu += 2;
        }
        else
        {
          _baseMaskFilter &= NoChantaCallsFilter;
          Fu += 4;
        }

        if (suitId == 2)
        {
          if (index % 2 == 0 && index != 2)
          {
            _baseMaskFilter &= RyuuiisouFilter;
          }
        }
        else
        {
          _baseMaskFilter &= RyuuiisouFilter;
        }
      }

      UpdateSuit(suitId, base5Hash);
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

    public void Init(int[] base5Hashes)
    {
      UpdateSuit(0, base5Hashes[0]);
      UpdateSuit(1, base5Hashes[1]);
      UpdateSuit(2, base5Hashes[2]);
      UpdateSuit(3, base5Hashes[3]);
    }

    public int FuFootprint(int suitId, int index)
    {
      return FuFootprintsLookup[_fuFootprintOffsets[suitId] + index];
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

    private const long ClosedYakuFilter = ~((1L << BitIndex.ClosedSanshokuDoujun) | (1L << BitIndex.Iipeikou) |
                                            (1L << BitIndex.Chiitoitsu) | (1L << BitIndex.Ryanpeikou) |
                                            (1L << BitIndex.ClosedHonitsu) | (1L << BitIndex.ClosedChinitsu) |
                                            (1L << BitIndex.ClosedTanyao) | (1L << BitIndex.MenzenTsumo) |
                                            (1L << BitIndex.Pinfu) | (1L << BitIndex.ClosedChanta) |
                                            (1L << BitIndex.ClosedJunchan) | (1L << BitIndex.ClosedIttsuu));

    private const long OpenYakuFilter = ~((1L << BitIndex.OpenSanshokuDoujun) | (1L << BitIndex.OpenHonitsu) | (1L << BitIndex.OpenChinitsu) |
                                          (1L << BitIndex.OpenTanyao) | (1L << BitIndex.OpenChanta) | (1L << BitIndex.OpenJunchan) |
                                          (1L << BitIndex.OpenIttsuu));

    private const long RyuuiisouFilter = ~(1L << BitIndex.Ryuuiisou);

    private const long HonorCallFilter = ~((1L << BitIndex.ClosedChinitsu) | (1L << BitIndex.OpenChinitsu) |
                                           (1L << BitIndex.ClosedJunchan) | (1L << BitIndex.OpenJunchan));

    private const long ChinroutouCallFilter = ~(1L << BitIndex.Chinroutou);

    private const long HonroutouCallFilter = ~(1L << BitIndex.Honroutou);

    private const long NoChantaCallsFilter = ~((1L << BitIndex.ClosedTanyao) | (1L << BitIndex.OpenTanyao));

    private const long OnlyChantaCallsFilter = ~((1L << BitIndex.ClosedChanta) | (1L << BitIndex.OpenChanta) |
                                                 (1L << BitIndex.ClosedJunchan) | (1L << BitIndex.OpenJunchan));

    private const long NoAnkanYakuFilter = ~(1L << BitIndex.Pinfu);
    private const int SuitHonitsuOffset = 20;
    private const int HonorTsuuiisouOffset = BitIndex.Tsuuiisou - 3;
    private const int HonorChantaOffset = 27;
    private const int HonorJikazeOffset = 54;
    private const int HonorBakazeOffset = 58;
    private const int HonorHakuHatsuChunOffset = 15;
    private const int HonorHonitsuChinitsuOffset = 20;
    private const int HonorRyuuiisouOffset = 28;
    private const int HonorDaisangenOffset = 6;
    private const int HonorShousangenOffset = 51;
    private const int HonorShousuushiiOffset = 9;
    private const int HonorDaisuushiiOffset = 12;
    private const int SuitIttsuuOffset = 44;

    private static readonly long[] HonorSumLookup;
    private static readonly long[] HonorOrLookup;
    private static readonly long[] HonorWaitShiftLookup;
    private static readonly long[] SuitOrLookup;
    private static readonly long[] SuitWaitShiftLookup;
    private static readonly byte[] FuFootprintsLookup;

    private readonly long[] _meldLookupValues = new long[4];

    private long _baseMask;
    private long _baseMaskFilter;

    private readonly int[] _fuFootprintOffsets = new int[3];

    private void UpdateSuit(int suitId, int base5Hash)
    {
      if (suitId == 3)
      {
        WaitShiftValues[3] = HonorWaitShiftLookup[base5Hash];
        HonorOr = HonorOrLookup[base5Hash] | (_meldLookupValues[3] & ~0b1_111_111_111_111L);
        HonorSum = HonorSumLookup[base5Hash] + _meldLookupValues[3];
      }
      else
      {
        WaitShiftValues[suitId] = SuitWaitShiftLookup[base5Hash];
        var suitOr = SuitOrLookup[base5Hash] | _meldLookupValues[suitId];
        SuitOr[suitId] = suitOr + (1L << (BitIndex.ClosedIttsuu - 3));
        _fuFootprintOffsets[suitId] = (int)((WaitShiftValues[suitId] >> 10) & 0b1111111111111L) * 680;
      }
    }
  }
}