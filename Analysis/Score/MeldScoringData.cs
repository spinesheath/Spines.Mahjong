using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;

namespace Spines.Mahjong.Analysis.Score
{
  internal class MeldScoringData
  {
    public MeldScoringData(IReadOnlyList<State.Meld> melds)
    {
      CalculateFilters(melds);
      CalculateLookupValues(melds);

      MeldLookupValues = _lookupValues;
    }
    
    public long FinalMask { get; private set; }

    public int KanCount { get; private set; }

    public IReadOnlyList<long> MeldLookupValues { get; }

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

    private readonly long[] _lookupValues = new long[4];

    private void CalculateLookupValues(IReadOnlyList<State.Meld> melds)
    {
      const int suitHonitsuOffset = 20;
      const int suitIttsuuOffset = 44;
      const int honorTsuuiisouOffset = 2;
      const int honorChantaOffset = 27;
      const int honorJikazeOffset = 54;
      const int honorBakazeOffset = 58;
      const int honorHakuHatsuChunOffset = 15;
      const int honorHonitsuChinitsuOffset = 20;
      const int honorRyuuiisouOffset = 28;
      const int honorDaisangenOffset = 6;
      const int honorShousangenOffset = 51;
      const int honorShousuushiiOffset = 9;
      const int honorDaisuushiiOffset = 12;

      _lookupValues[3] |= 4L << honorRyuuiisouOffset;

      foreach (var meld in melds)
      {
        var tileType = meld.LowestTile.TileType;
        var suit = tileType.Suit;
        var index = tileType.Index;

        if (suit == Suit.Jihai)
        {
          _lookupValues[3] |= 1L << honorChantaOffset;
          _lookupValues[3] |= 1L << honorHonitsuChinitsuOffset;
          _lookupValues[3] += 1L << honorTsuuiisouOffset;

          if (index < 4)
          {
            _lookupValues[3] |= 1L << (honorJikazeOffset + index);
            _lookupValues[3] |= 1L << (honorBakazeOffset + index);
            _lookupValues[3] += 1L << honorShousuushiiOffset;
            _lookupValues[3] += 1L << honorDaisuushiiOffset;
            _lookupValues[3] &= ~(1L << (honorShousuushiiOffset + 2));
          }
          else
          {
            _lookupValues[3] |= 1L << (honorHakuHatsuChunOffset + index - 4);
            _lookupValues[3] += 1L << honorShousangenOffset;
            _lookupValues[3] += 1L << honorDaisangenOffset;
          }

          if (index != 5)
          {
            _lookupValues[3] &= ~(4L << honorRyuuiisouOffset);
          }
        }
        else
        {
          _lookupValues[tileType.SuitId] |= 0b101000L << suitHonitsuOffset;

          if (meld.MeldType == MeldType.Shuntsu)
          {
            if (index % 3 == 0)
            {
              _lookupValues[tileType.SuitId] |= 1L << (suitIttsuuOffset + index / 3);
            }

            _lookupValues[tileType.SuitId] |= index + 4L;
            _lookupValues[tileType.SuitId] |= 1L << (index + 6);
          }
          else
          {
            _lookupValues[tileType.SuitId] |= index + 9L;
            _lookupValues[tileType.SuitId] |= 1L << (index + 13);
          }
        }
      }
    }

    private void CalculateFilters(IReadOnlyList<State.Meld> melds)
    {
      BigAndToSumFilter = (0b1L << BitIndex.Toitoi) | (0b1L << BitIndex.ClosedChanta);
      SankantsuSuukantsu = 1L << (BitIndex.Sankantsu - 3);

      var x = ~0L;
      var baseMask = OpenYakuFilter;
      foreach (var meld in melds)
      {
        var suit = meld.LowestTile.TileType.Suit;
        var index = meld.LowestTile.TileType.Index;

        if (meld.MeldType == MeldType.ClosedKan)
        {
          x &= NoAnkanYakuFilter;
          ShiftedAnkanCount += 1L << (BitIndex.Sanankou - 2);
        }
        else
        {
          baseMask = ClosedYakuFilter;
          OpenBit = 1L;
        }

        if (meld.MeldType == MeldType.Shuntsu)
        {
          BigAndToSumFilter = 0b1L << BitIndex.ClosedChanta;
        }

        if (meld.IsKan)
        {
          KanCount += 1;
          SankantsuSuukantsu <<= 1;
        }

        if (suit == Suit.Jihai)
        {
          x &= HonorCallFilter;
          x &= ChinroutouCallFilter;

          if (index != 5)
          {
            x &= RyuuiisouFilter;
          }
        }
        else
        {
          if (meld.MeldType == MeldType.Shuntsu)
          {
            x &= ChinroutouCallFilter;
            x &= HonroutouCallFilter;
          }
          else if (index > 0 && index < 8)
          {
            x &= ChinroutouCallFilter;
            x &= HonroutouCallFilter;
          }

          if (suit == Suit.Souzu)
          {
            if (meld.MeldType == MeldType.Shuntsu && index != 1)
            {
              x &= RyuuiisouFilter;
            }

            if (meld.MeldType != MeldType.Shuntsu && index % 2 == 0 && index != 2)
            {
              x &= RyuuiisouFilter;
            }
          }
          else
          {
            x &= RyuuiisouFilter;
          }
        }

        if (meld.Tiles.Any(t => t.TileType.IsKyuuhai))
        {
          x &= NoChantaCallsFilter;
        }
        else
        {
          x &= OnlyChantaCallsFilter;
        }
      }

      FinalMask = baseMask & x;
    }
  }
}