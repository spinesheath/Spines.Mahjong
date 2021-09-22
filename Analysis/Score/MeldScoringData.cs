using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Resources;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Score
{
  internal class MeldScoringData
  {
    static MeldScoringData()
    {
      HonorMeldSumLookup = Resource.LongLookup("Scoring", "HonorMeldSumLookup.dat");
      SuitMeldOrLookup = Resource.LongLookup("Scoring", "SuitMeldOrLookup.dat");
    }

    public MeldScoringData(HandCalculator hand, IReadOnlyList<State.Meld> melds)
    {
      CalculateFilters(melds);

      var lookupValues = new long[4];
      lookupValues[0] = SuitMeldOrLookup[MeldIndex(hand, 0)];
      lookupValues[1] = SuitMeldOrLookup[MeldIndex(hand, 1)];
      lookupValues[2] = SuitMeldOrLookup[MeldIndex(hand, 2)];
      lookupValues[3] = HonorMeldSumLookup[MeldIndex(hand, 3)];

      MeldLookupValues = lookupValues;
    }

    public long AnkanYakuFilter { get; private set; }

    public long FinalMask { get; private set; }

    public int KanCount { get; private set; }

    public IReadOnlyList<long> MeldLookupValues { get; }

    public long OpenBit { get; private set; }

    public long ShiftedAnkanCount { get; private set; }

    public long ToitoiFilter { get; private set; }

    private const long ClosedYakuFilter = ~((1L << BitIndex.ClosedSanshokuDoujun) | (1L << BitIndex.Iipeikou) |
                                            (1L << BitIndex.Chiitoitsu) | (1L << BitIndex.Ryanpeikou) |
                                            (1L << BitIndex.ClosedHonitsu) | (1L << BitIndex.ClosedChinitsu) |
                                            (1L << BitIndex.ClosedTanyao) | (1L << BitIndex.MenzenTsumo) |
                                            (1L << BitIndex.Pinfu) | (1L << BitIndex.ClosedChanta) |
                                            (1L << BitIndex.ClosedJunchan) | (1L << BitIndex.ClosedIttsuu));

    private const long OpenYakuFilter = ~((1L << BitIndex.OpenSanshokuDoujun) | (1L << BitIndex.OpenHonitsu) | (1L << BitIndex.OpenChinitsu) |
                                          (1L << BitIndex.OpenTanyao) | (1L << BitIndex.OpenChanta) | (1L << BitIndex.OpenJunchan) |
                                          (1L << BitIndex.OpenIttsuu));

    private const long NoChiiYakuFilter = ~(1L << BitIndex.Toitoi);

    private const long RyuuiisouFilter = ~(1L << BitIndex.Ryuuiisou);

    private const long HonorCallFilter = ~((1L << BitIndex.ClosedChinitsu) | (1L << BitIndex.OpenChinitsu) |
                                           (1L << BitIndex.ClosedJunchan) | (1L << BitIndex.OpenJunchan));

    private const long ChinroutouCallFilter = ~(1L << BitIndex.Chinroutou);

    private const long HonroutouCallFilter = ~(1L << BitIndex.Honroutou);

    private const long NoChantaCallsFilter = ~((1L << BitIndex.ClosedTanyao) | (1L << BitIndex.OpenTanyao));

    private const long OnlyChantaCallsFilter = ~((1L << BitIndex.ClosedChanta) | (1L << BitIndex.OpenChanta) |
                                                 (1L << BitIndex.ClosedJunchan) | (1L << BitIndex.OpenJunchan));

    private const long NoAnkanYakuFilter = ~((1L << BitIndex.Pinfu) | (1L << BitIndex.Chiitoitsu));

    private static readonly long[] HonorMeldSumLookup;

    private static readonly long[] SuitMeldOrLookup;

    private void CalculateFilters(IReadOnlyList<State.Meld> melds)
    {
      ToitoiFilter = ~0L;
      AnkanYakuFilter = ~0L;

      var x = ~0L;
      var baseMask = OpenYakuFilter;
      foreach (var meld in melds)
      {
        var suit = meld.LowestTile.TileType.Suit;
        var index = meld.LowestTile.TileType.Index;

        if (meld.MeldType == MeldType.ClosedKan)
        {
          AnkanYakuFilter = NoAnkanYakuFilter;
          ShiftedAnkanCount += 1L << (BitIndex.Sanankou - 2);
        }
        else
        {
          baseMask = ClosedYakuFilter;
          OpenBit = 1L;
        }

        if (meld.MeldType == MeldType.Shuntsu)
        {
          ToitoiFilter = NoChiiYakuFilter;
        }

        if (meld.IsKan)
        {
          KanCount += 1;
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

    private static int MeldIndex(HandCalculator hand, int suitId)
    {
      var meldIndex = 0;
      foreach (var i in hand.MeldIds(suitId))
      {
        meldIndex *= 35;
        meldIndex += i + 1;
      }

      return meldIndex;
    }
  }
}