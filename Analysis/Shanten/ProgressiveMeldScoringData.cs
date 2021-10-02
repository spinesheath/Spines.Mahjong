using System;
using System.Collections.Generic;
using Spines.Mahjong.Analysis.Score;

namespace Spines.Mahjong.Analysis.Shanten
{
  internal class ProgressiveMeldScoringData : IMeldScoringData
  {
    public ProgressiveMeldScoringData()
    {
      _lookupValues[3] |= 4L << HonorRyuuiisouOffset;

      BigAndToSumFilter = (0b1L << BitIndex.Toitoi) | (0b1L << BitIndex.ClosedChanta);
      SankantsuSuukantsu = 1L << (BitIndex.Sankantsu - 3);

      _baseMaskFilter = ~0L;
      _baseMask = OpenYakuFilter;
    }

    public void Ankan(TileType tileType)
    {
      var suit = tileType.Suit;
      var index = tileType.Index;

      if (suit == Suit.Jihai)
      {
        _lookupValues[3] |= 1L << HonorChantaOffset;
        _lookupValues[3] |= 1L << HonorHonitsuChinitsuOffset;
        _lookupValues[3] += 1L << HonorTsuuiisouOffset;

        if (index < 4)
        {
          _lookupValues[3] |= 1L << (HonorJikazeOffset + index);
          _lookupValues[3] |= 1L << (HonorBakazeOffset + index);
          _lookupValues[3] += 1L << HonorShousuushiiOffset;
          _lookupValues[3] += 1L << HonorDaisuushiiOffset;
          _lookupValues[3] &= ~(1L << (HonorShousuushiiOffset + 2));
        }
        else
        {
          _lookupValues[3] |= 1L << (HonorHakuHatsuChunOffset + index - 4);
          _lookupValues[3] += 1L << HonorShousangenOffset;
          _lookupValues[3] += 1L << HonorDaisangenOffset;
        }

        if (index != 5)
        {
          _lookupValues[3] &= ~(4L << HonorRyuuiisouOffset);
        }
      }
      else
      {
        _lookupValues[tileType.SuitId] |= 0b101000L << SuitHonitsuOffset;
        _lookupValues[tileType.SuitId] |= index + 9L;
        _lookupValues[tileType.SuitId] |= 1L << (index + 13);
      }


      _baseMaskFilter &= NoAnkanYakuFilter;
      ShiftedAnkanCount += 1L << (BitIndex.Sanankou - 2);
      
      SankantsuSuukantsu <<= 1;

      if (suit == Suit.Jihai)
      {
        _baseMaskFilter &= HonorCallFilter;
        _baseMaskFilter &= ChinroutouCallFilter;

        if (index != 5)
        {
          _baseMaskFilter &= RyuuiisouFilter;
        }
      }
      else
      {
        if (index > 0 && index < 8)
        {
          _baseMaskFilter &= ChinroutouCallFilter;
          _baseMaskFilter &= HonroutouCallFilter;
        }

        if (suit == Suit.Souzu)
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

      if (tileType.IsKyuuhai)
      {
        _baseMaskFilter &= NoChantaCallsFilter;
      }
      else
      {
        _baseMaskFilter &= OnlyChantaCallsFilter;
      }
    }

    public void Chii(TileType tileType)
    {
      const int suitHonitsuOffset = 20;
      const int suitIttsuuOffset = 44;

      var index = tileType.Index;
      var suit = tileType.Suit;

      _lookupValues[tileType.SuitId] |= 0b101000L << suitHonitsuOffset;

      if (index % 3 == 0)
      {
        _lookupValues[tileType.SuitId] |= 1L << (suitIttsuuOffset + index / 3);
      }

      _lookupValues[tileType.SuitId] |= index + 4L;
      _lookupValues[tileType.SuitId] |= 1L << (index + 6);


      _baseMask = ClosedYakuFilter;
      OpenBit = 1L;

      BigAndToSumFilter = 0b1L << BitIndex.ClosedChanta;

      _baseMaskFilter &= ChinroutouCallFilter;
      _baseMaskFilter &= HonroutouCallFilter;

      if (suit == Suit.Souzu)
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
    }

    public ProgressiveMeldScoringData Clone()
    {
      var c = new ProgressiveMeldScoringData
      {
        _baseMask = _baseMask,
        _baseMaskFilter = _baseMaskFilter,
        OpenBit = OpenBit,
        ShiftedAnkanCount = ShiftedAnkanCount,
        BigAndToSumFilter = BigAndToSumFilter,
        SankantsuSuukantsu = SankantsuSuukantsu
      };

      Array.Copy(_lookupValues, c._lookupValues, _lookupValues.Length);
      return c;
    }

    public void Daiminkan(TileType tileType)
    {
      var suit = tileType.Suit;
      var index = tileType.Index;

      if (suit == Suit.Jihai)
      {
        _lookupValues[3] |= 1L << HonorChantaOffset;
        _lookupValues[3] |= 1L << HonorHonitsuChinitsuOffset;
        _lookupValues[3] += 1L << HonorTsuuiisouOffset;

        if (index < 4)
        {
          _lookupValues[3] |= 1L << (HonorJikazeOffset + index);
          _lookupValues[3] |= 1L << (HonorBakazeOffset + index);
          _lookupValues[3] += 1L << HonorShousuushiiOffset;
          _lookupValues[3] += 1L << HonorDaisuushiiOffset;
          _lookupValues[3] &= ~(1L << (HonorShousuushiiOffset + 2));
        }
        else
        {
          _lookupValues[3] |= 1L << (HonorHakuHatsuChunOffset + index - 4);
          _lookupValues[3] += 1L << HonorShousangenOffset;
          _lookupValues[3] += 1L << HonorDaisangenOffset;
        }

        if (index != 5)
        {
          _lookupValues[3] &= ~(4L << HonorRyuuiisouOffset);
        }
      }
      else
      {
        _lookupValues[tileType.SuitId] |= 0b101000L << SuitHonitsuOffset;
        _lookupValues[tileType.SuitId] |= index + 9L;
        _lookupValues[tileType.SuitId] |= 1L << (index + 13);
      }

      _baseMask = ClosedYakuFilter;
      OpenBit = 1L;
      
      SankantsuSuukantsu <<= 1;

      if (suit == Suit.Jihai)
      {
        _baseMaskFilter &= HonorCallFilter;
        _baseMaskFilter &= ChinroutouCallFilter;

        if (index != 5)
        {
          _baseMaskFilter &= RyuuiisouFilter;
        }
      }
      else
      {
        if (index > 0 && index < 8)
        {
          _baseMaskFilter &= ChinroutouCallFilter;
          _baseMaskFilter &= HonroutouCallFilter;
        }

        if (suit == Suit.Souzu)
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

      if (tileType.IsKyuuhai)
      {
        _baseMaskFilter &= NoChantaCallsFilter;
      }
      else
      {
        _baseMaskFilter &= OnlyChantaCallsFilter;
      }
    }

    public void Pon(TileType tileType)
    {
      var suit = tileType.Suit;
      var index = tileType.Index;

      if (suit == Suit.Jihai)
      {
        _lookupValues[3] |= 1L << HonorChantaOffset;
        _lookupValues[3] |= 1L << HonorHonitsuChinitsuOffset;
        _lookupValues[3] += 1L << HonorTsuuiisouOffset;

        if (index < 4)
        {
          _lookupValues[3] |= 1L << (HonorJikazeOffset + index);
          _lookupValues[3] |= 1L << (HonorBakazeOffset + index);
          _lookupValues[3] += 1L << HonorShousuushiiOffset;
          _lookupValues[3] += 1L << HonorDaisuushiiOffset;
          _lookupValues[3] &= ~(1L << (HonorShousuushiiOffset + 2));
        }
        else
        {
          _lookupValues[3] |= 1L << (HonorHakuHatsuChunOffset + index - 4);
          _lookupValues[3] += 1L << HonorShousangenOffset;
          _lookupValues[3] += 1L << HonorDaisangenOffset;
        }

        if (index != 5)
        {
          _lookupValues[3] &= ~(4L << HonorRyuuiisouOffset);
        }
      }
      else
      {
        _lookupValues[tileType.SuitId] |= 0b101000L << SuitHonitsuOffset;
        _lookupValues[tileType.SuitId] |= index + 9L;
        _lookupValues[tileType.SuitId] |= 1L << (index + 13);
      }


      _baseMask = ClosedYakuFilter;
      OpenBit = 1L;

      if (suit == Suit.Jihai)
      {
        _baseMaskFilter &= HonorCallFilter;
        _baseMaskFilter &= ChinroutouCallFilter;

        if (index != 5)
        {
          _baseMaskFilter &= RyuuiisouFilter;
        }
      }
      else
      {
        if (index > 0 && index < 8)
        {
          _baseMaskFilter &= ChinroutouCallFilter;
          _baseMaskFilter &= HonroutouCallFilter;
        }

        if (suit == Suit.Souzu)
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

      if (tileType.IsKyuuhai)
      {
        _baseMaskFilter &= NoChantaCallsFilter;
      }
      else
      {
        _baseMaskFilter &= OnlyChantaCallsFilter;
      }
    }

    public void Shouminkan()
    {
      SankantsuSuukantsu <<= 1;
    }

    public long FinalMask => _baseMask & _baseMaskFilter;

    public IReadOnlyList<long> MeldLookupValues => _lookupValues;

    public long OpenBit { get; private set; }

    public long ShiftedAnkanCount { get; private set; }

    public long BigAndToSumFilter { get; private set; }

    public long SankantsuSuukantsu { get; private set; }

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
    private const int HonorTsuuiisouOffset = 2;
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

    private readonly long[] _lookupValues = new long[4];

    private long _baseMask;
    private long _baseMaskFilter;
  }
}