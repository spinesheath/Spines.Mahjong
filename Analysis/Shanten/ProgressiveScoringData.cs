using System;
using Spines.Mahjong.Analysis.Resources;
using Spines.Mahjong.Analysis.Score;

namespace Spines.Mahjong.Analysis.Shanten
{
  public class ProgressiveScoringData
  {
    static ProgressiveScoringData()
    {
      LookupHonorSum = Resource.LongLookup("Scoring", "HonorSumLookup.dat");
      LookupHonorOr = Resource.LongLookup("Scoring", "HonorOrLookup.dat");
      LookupHonorWaitShift = Resource.LongLookup("Scoring", "HonorWaitShiftLookup.dat");

      LookupSuitOr = Resource.LongLookup("Scoring", "SuitOrLookup.dat");
      LookupSuitWaitShift = Resource.LongLookup("Scoring", "SuitWaitShiftLookup.dat");

      LookupFuFootprints = Resource.Lookup("Scoring", "SuitFu.dat");

      SuitWaitShift0 = LookupSuitWaitShift[0];
      SuitOr0 = LookupSuitOr[0];
      HonorWaitShift0 = LookupHonorWaitShift[0];
    }

    public ProgressiveScoringData()
    {
      _meldLookupValues[3] |= 4L << OffsetHonorRyuuiisou;

      _bigAndToSumFilter = (0b1L << BitIndex.Toitoi) | (0b1L << BitIndex.ClosedChanta);
      _sankantsuSuukantsu = 1L << (BitIndex.Sankantsu - 3);

      _baseMaskFilter = ~0L;
      _baseMask = FilterOpenYaku;

      _honorOr = LookupHonorOr[0] | (_meldLookupValues[3] & ~0b1_111_111_111_111L);

      _honorSum = LookupHonorSum[0] + _meldLookupValues[3];

      _baseAndMeldFu = 20;
    }

    public void Ankan(int suitId, int index, int base5Hash)
    {
      _baseMaskFilter &= FilterNoAnkanYaku;
      _sankantsuSuukantsu <<= 1;
      _shiftedAnkanCount += 1L << (BitIndex.Sanankou - 2);
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
      _openBit = 1L;

      _bigAndToSumFilter = 0b1L << BitIndex.ClosedChanta;

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
        _openBit = _openBit,
        _shiftedAnkanCount = _shiftedAnkanCount,
        _bigAndToSumFilter = _bigAndToSumFilter,
        _sankantsuSuukantsu = _sankantsuSuukantsu,
        _honorSum = _honorSum,
        _honorOr = _honorOr,
        _baseAndMeldFu = _baseAndMeldFu
      };

      Array.Copy(_meldLookupValues, c._meldLookupValues, _meldLookupValues.Length);
      Array.Copy(_waitShiftValues, c._waitShiftValues, _waitShiftValues.Length);
      Array.Copy(_suitOr, c._suitOr, _suitOr.Length);
      Array.Copy(_fuFootprintOffsets, c._fuFootprintOffsets, c._fuFootprintOffsets.Length);

      return c;
    }

    public void Daiminkan(int suitId, int index, int base5Hash)
    {
      _baseMask = FilterClosedYaku;
      _sankantsuSuukantsu <<= 1;
      _openBit = 1L;
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

    public int FuFootprint(int suitId, int index)
    {
      return LookupFuFootprints[_fuFootprintOffsets[suitId] + index];
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
      _openBit = 1L;
      AnyKoutsu(suitId, index, base5Hash, 2);
    }

    public void Shouminkan(TileType tileType, int base5Hash)
    {
      _sankantsuSuukantsu <<= 1;

      if (tileType.IsKyuuhai)
      {
        _baseAndMeldFu += 12; // Upgrade from pon to kan
      }
      else
      {
        _baseAndMeldFu += 6; // Upgrade from pon to kan
      }

      UpdateSuit(tileType.SuitId, base5Hash);
    }

    public (long, int) YakuAndFu(WindScoringData windScoringData, TileType winningTile, bool isRon)
    {
      // TODO might be able to rework ron shift to not use up so many bits.
      var ronShiftAmount = isRon ? 9 : 0;

      var winningTileIndex = winningTile.Index;
      var winningTileSuit = winningTile.SuitId;

      Span<long> waitShiftValues = stackalloc long[4];
      _waitShiftValues.CopyTo(waitShiftValues);
      waitShiftValues[winningTileSuit] >>= winningTileIndex + 1;

      var suitsAnd = _suitOr[0] & _suitOr[1] & _suitOr[2];

      var bigSum = (_suitOr[0] & SuitBigSumFilter) +
                   (_suitOr[1] & SuitBigSumFilter) +
                   (_suitOr[2] & SuitBigSumFilter) +
                   (_honorOr & HonorBigSumFilter);

      var sanshokuShift = (int) suitsAnd & 0b11111;
      var sanshoku = (suitsAnd >> sanshokuShift) & SanshokuYakuFilter;

      /*
       * Pinfu
       * Honors are shifted by an amount based on value winds to make sure only guest wind pairs are possible
       * The suit with the winning tile is shifted by the drawn tile to ensure ryanmen wait and non-honor wait (also used for other yaku)
       * After that, some constellations where pinfu is not possible because of other yaku locking shapes are eliminated:
       * Sanankou and sanshoku. Ittsuu is a single suit issue and has been dealt with in the lookup preparation.
       * Some chiitoitsu hands evaluate to pinfu by the previous steps, despite being clearly not pinfu.
       * A flag in BigSum is created by adding 1 for each suit with a pair and a 2 for honors.
       * This will leave a 0 in the second bit in the bad case: 11223399m11p11s44z This 0 is aligned with the pinfu bit index.
       */
      var honorWindShift = _honorOr >> windScoringData.HonorShift;
      var waitAndWindShift = waitShiftValues[0] & waitShiftValues[1] & waitShiftValues[2] & waitShiftValues[3] & honorWindShift;
      var pinfu = waitAndWindShift &
                  // TODO suitsAnd + 1 should instead be handled in bitField preparation
                  (_suitOr[winningTileSuit] >> (int) ((winningTileIndex + ((suitsAnd + 1) & 1)) * (sanshoku >> 6))) &
                  bigSum &
                  PinfuYakuFilter;

      var tankiBit = waitShiftValues[winningTileSuit] & 0b1L;

      // get this before ron shifting
      // TODO get rid of this conditional
      var singleWaitFuJihai = winningTileSuit == 3 ? (waitShiftValues[winningTileSuit] >> 9) & 2L : 0;

      // TODO waitAndRonShift is only used for ankou now, so maybe shifting inside the array is not necessary anymore?
      waitShiftValues[winningTileSuit] >>= ronShiftAmount;

      var waitAndRonShift = (waitShiftValues[0] & RonShiftSumFilter) +
                            (waitShiftValues[1] & RonShiftSumFilter) +
                            (waitShiftValues[2] & RonShiftSumFilter) +
                            (waitShiftValues[3] & RonShiftSumFilter);

      waitAndRonShift += _shiftedAnkanCount;

      waitAndRonShift += bigSum & (0b111L << AnkouRonShiftSumFilterIndex);
      waitAndRonShift += waitAndRonShift & (0b101L << AnkouRonShiftSumFilterIndex);

      var bigAnd = suitsAnd & _honorOr;

      var result = 0L;

      result |= ((1L << BitIndex.MenzenTsumo) >> ronShiftAmount) & (1L << BitIndex.MenzenTsumo);
      result |= waitAndRonShift & AnkouYakuFilter;
      result |= sanshoku;
      result |= pinfu;
      result |= bigAnd & BigAndYakuFilter;

      bigSum |= bigAnd & _bigAndToSumFilter;

      var bigSumPostElimination = bigSum & ~((bigSum & BigSumEliminationFilter) >> EliminationDelta);
      result |= bigSumPostElimination & BigSumPostEliminationYakuFilter;

      result |= _honorSum & HonorSumYakuFilter & windScoringData.ValueWindFilter;

      result |= _sankantsuSuukantsu & (11L << BitIndex.Sankantsu);

      var ryuuiisouSum = (_suitOr[0] & RyuuiisouSumFilter01) +
                         (_suitOr[1] & RyuuiisouSumFilter01) +
                         (_suitOr[2] & RyuuiisouSumFilter2) +
                         _honorSum;
      result |= ryuuiisouSum & (1L << BitIndex.Ryuuiisou);

      if ((result & (1L << BitIndex.Chiitoitsu)) != 0)
      {
        result &= ~((1L << BitIndex.ClosedChanta) | (1L << BitIndex.Iipeikou));
      }

      var openIipeikouBit = (result >> BitIndex.Iipeikou) & 1L;
      var sanankouBit = (result >> BitIndex.Sanankou) & 1L;

      // get this before shifting to open ssk
      var sanshokuFuMultiplier = (int) (((result >> BitIndex.SanshokuDoukou) + (result >> BitIndex.ClosedSanshokuDoujun)) & 1);

      result += (result & OpenBitFilter) * _openBit;

      var closedChantaBit = (result >> BitIndex.ClosedChanta) & 1L;
      var closedJunchanBit = (result >> BitIndex.ClosedJunchan) & 1L;
      var openJunchanBit = (result >> BitIndex.OpenJunchan) & 1L;
      var toitoiBit = (result >> BitIndex.Toitoi) & 1L;

      var x = openIipeikouBit & (closedChantaBit | closedJunchanBit);
      var removeSequenceYaku = (sanankouBit ^ x) & sanankouBit;
      var removeOpenJunchan = openIipeikouBit & (sanankouBit | toitoiBit) & openJunchanBit;
      var removeSanankou = x * (1 - toitoiBit);
      result -= (result & (1L << BitIndex.Sanankou)) * removeSanankou;
      // (openIipeikouBit << BitIndex.OpenChanta) means 111222333 shape and chanta, here excluded in case of sanankou
      result -= (result & ((1L << BitIndex.Pinfu) | (1L << BitIndex.Iipeikou) | (openIipeikouBit << BitIndex.OpenChanta))) * removeSequenceYaku;
      result -= (result & (1L << BitIndex.OpenJunchan)) * removeOpenJunchan;
      result -= (result & ((1L << BitIndex.Iipeikou) | (1L << BitIndex.ClosedJunchan))) * (toitoiBit & (1 - _openBit));

      result += (result & TankiUpgradeableFilter) * tankiBit;

      // This covers the 22234555m222p222s case where sanankou/sanshoku doukou depend on the wait.
      var sanankouAndDoukou = (_suitOr[winningTileSuit] >> (BitIndex.Sanankou - BitIndex.SanshokuDoukou + 1)) & (1L << BitIndex.SanshokuDoukou);
      var waitPreventsDoukou = (suitsAnd >> (winningTileIndex + ronShiftAmount - 9)) & sanankouAndDoukou;
      result -= waitPreventsDoukou & (result >> (BitIndex.Sanankou - BitIndex.SanshokuDoukou));

      result &= _baseMask & _baseMaskFilter;

      var yakuman = result & YakumanFilter;
      if (yakuman != 0)
      {
        return (yakuman, 0);
      }

      if ((result & (1L << BitIndex.Chiitoitsu)) != 0)
      {
        return (result, 25);
      }

      var closedRonFu = (int) (1 - _openBit) * (10 >> (9 - ronShiftAmount));

      if ((result & PinfuYakuFilter) != 0)
      {
        return (result, 20 + closedRonFu);
      }

      var squareTypeToShuntsu = ((ryuuiisouSum & ~result) >> BitIndex.Sanankou) & ((1L - _openBit) | (result >> BitIndex.OpenJunchan) | ((result >> BitIndex.OpenChanta) & 1L));

      var footprintKey = (sanshokuShift + 1) * 40 * sanshokuFuMultiplier;
      footprintKey += (int) squareTypeToShuntsu * 40;
      footprintKey |= ronShiftAmount & 1; // ronShiftAmount is either 0 or 0b1001
      footprintKey |= (int) _openBit << 1;

      Span<int> keys = stackalloc int[] {footprintKey, footprintKey, footprintKey, 0};
      keys[winningTileSuit] += (winningTileIndex + 1) * 4;

      var fuM = FuFootprint(0, keys[0]);
      var fuP = FuFootprint(1, keys[1]);
      var fuS = FuFootprint(2, keys[2]);

      // TODO jihai single wait and ankou fu the same way as suits, maybe with value wind info instead of ssk, incorporating valuePairFu
      var bonusAnkouCountWinningSuit = (waitShiftValues[winningTileSuit] >> 32) & 1L;
      var potentialBonusAnkouCountWinningSuit = (_waitShiftValues[winningTileSuit] >> 32) & 1L;
      var ankouFuCorrection = (potentialBonusAnkouCountWinningSuit - bonusAnkouCountWinningSuit) * ((0b100000 >> winningTileSuit) & 0b100);
      var ankouFuJihai = ((_waitShiftValues[3] >> 17) & (0b111L << 3)) - ankouFuCorrection;

      var tsumoFu = 2 >> ronShiftAmount;

      // lowest bit of honorOr is 1 iff there is a wind pair
      var valueWindAdjuster = 1 + (_honorOr & windScoringData.DoubleValueWindBit);
      var valuePairFu = (honorWindShift & 1L) << (int) valueWindAdjuster;

      var fu = _baseAndMeldFu + closedRonFu + tsumoFu + (int) valuePairFu + fuM + fuP + fuS + (int) ankouFuJihai + (int) singleWaitFuJihai;

      var r = fu % 10;
      var rounding = r == 0 && fu != 20 ? 0 : 10 - r;

      return (result, fu + rounding);
    }

    private const int EliminationDelta = 4;

    private const long SanshokuYakuFilter = (1L << BitIndex.ClosedSanshokuDoujun) | (1L << BitIndex.SanshokuDoukou);

    private const long BigAndYakuFilter = (1L << BitIndex.Honroutou) |
                                          (1L << BitIndex.ClosedTanyao) |
                                          (1L << BitIndex.Chinroutou) |
                                          (1L << BitIndex.ClosedJunchan);

    private const long BigSumPostEliminationYakuFilter = (1L << BitIndex.Iipeikou) | (1L << BitIndex.Chiitoitsu) | (1L << BitIndex.Ryanpeikou) |
                                                         (1L << BitIndex.ClosedChinitsu) | (1L << BitIndex.ClosedHonitsu) |
                                                         (1L << BitIndex.ChuurenPoutou) |
                                                         (1L << BitIndex.ClosedChanta) | (1L << BitIndex.Toitoi) |
                                                         (1L << BitIndex.ClosedIttsuu);

    private const long BigSumEliminationFilter = (1L << (BitIndex.ClosedChinitsu + 4)) | (1L << (BitIndex.OpenChinitsu + 4)) |
                                                 (1L << (BitIndex.ClosedHonitsu + 4)) | (1L << (BitIndex.OpenHonitsu + 4)) |
                                                 (1L << BitIndex.Toitoi);

    private const long HonorSumYakuFilter = (1L << BitIndex.Haku) | (1L << BitIndex.Hatsu) | (1L << BitIndex.Chun) |
                                            (1L << BitIndex.JikazeTon) | (1L << BitIndex.JikazeNan) |
                                            (1L << BitIndex.JikazeShaa) | (1L << BitIndex.JikazePei) |
                                            (1L << BitIndex.BakazeTon) | (1L << BitIndex.BakazeNan) |
                                            (1L << BitIndex.BakazeShaa) | (1L << BitIndex.BakazePei) |
                                            (1L << BitIndex.Shousangen) | (1L << BitIndex.Daisangen) |
                                            (1L << BitIndex.Shousuushi) | (1L << BitIndex.Daisuushi) |
                                            (1L << BitIndex.KokushiMusou) | (1L << BitIndex.Tsuuiisou);

    private const long YakumanFilter = (1L << BitIndex.Daisangen) | (1L << BitIndex.Shousuushi) | (1L << BitIndex.Daisuushi) |
                                       (1L << BitIndex.Suuankou) | (1L << BitIndex.SuuankouTanki) |
                                       (1L << BitIndex.KokushiMusou) | (1L << BitIndex.KokushiMusouJuusanmen) |
                                       (1L << BitIndex.Tsuuiisou) | (1L << BitIndex.Chinroutou) |
                                       (1L << BitIndex.ChuurenPoutou) | (1L << BitIndex.JunseiChuurenPoutou) |
                                       (1L << BitIndex.Suukantsu) | (1L << BitIndex.Ryuuiisou);

    private const long PinfuYakuFilter = 1L << BitIndex.Pinfu;

    private const int AnkouRonShiftSumFilterIndex = BitIndex.Sanankou - 2;
    private const long RonShiftSumFilter = 1L << AnkouRonShiftSumFilterIndex;
    private const long AnkouYakuFilter = (1L << BitIndex.Sanankou) | (1L << BitIndex.Suuankou) | (1L << BitIndex.SuuankouTanki);

    private const long SuitBigSumFilter = (0b11_00000_0101_0000L << 19) |
                                          (1L << (BitIndex.Pinfu - 1)) |
                                          (1L << BitIndex.ChuurenPoutou) |
                                          (0b1111L << (BitIndex.Chiitoitsu - 3)) |
                                          (0b11L << BitIndex.Iipeikou) |
                                          (1L << BitIndex.ClosedIttsuu);

    private const long HonorBigSumFilter = (0b11_00000_0000_1111L << 19) |
                                           (1L << BitIndex.Pinfu) |
                                           (0b1111L << (BitIndex.Chiitoitsu - 3));

    private const long TankiUpgradeableFilter = (1L << BitIndex.Suuankou) | (1L << BitIndex.KokushiMusou) | (1L << BitIndex.ChuurenPoutou);

    private const long OpenBitFilter = (1L << BitIndex.ClosedChinitsu) | (1L << BitIndex.ClosedHonitsu) | (1L << BitIndex.ClosedSanshokuDoujun) |
                                       (1L << BitIndex.ClosedTanyao) | (1L << BitIndex.ClosedChanta) | (1L << BitIndex.ClosedJunchan) |
                                       (1L << BitIndex.ClosedIttsuu);

    // Sanankou bit index added here for square type, since it does not interfere with ryuuiisou
    private const long RyuuiisouSumFilter01 = (1L << (BitIndex.Ryuuiisou - 4)) | (1L << BitIndex.Sanankou);
    private const long RyuuiisouSumFilter2 = (1L << (BitIndex.Ryuuiisou - 2)) | (1L << BitIndex.Sanankou);

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
    private static readonly long SuitWaitShift0;
    private static readonly long SuitOr0;
    private static readonly long HonorWaitShift0;

    private readonly int[] _fuFootprintOffsets = new int[3];
    private readonly long[] _meldLookupValues = new long[4];
    private readonly long[] _suitOr = {SuitOr0, SuitOr0, SuitOr0, 0L};
    private readonly long[] _waitShiftValues = {SuitWaitShift0, SuitWaitShift0, SuitWaitShift0, HonorWaitShift0};

    private long _baseMask;
    private long _baseMaskFilter;
    private long _bigAndToSumFilter;
    private int _baseAndMeldFu;
    private long _honorOr;
    private long _honorSum;
    private long _openBit;
    private long _sankantsuSuukantsu;
    private long _shiftedAnkanCount;

    private static string PrintBinarySegment(long bits, int from, int length)
    {
      return Convert.ToString((bits >> from) & ((1L << length) - 1), 2).PadLeft(length, '0');
    }

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
        _baseAndMeldFu += 2 * baseFu;
      }
      else
      {
        _meldLookupValues[suitId] |= (0b101000L << OffsetSuitHonitsu) | index | (1L << 5 << index);

        if (index > 0 && index < 8)
        {
          _baseMaskFilter &= FilterChinroutouCall & FilterHonroutouCall & FilterOnlyChantaCalls;
          _baseAndMeldFu += baseFu;
        }
        else
        {
          _baseMaskFilter &= FilterNoChantaCalls;
          _baseAndMeldFu += 2 * baseFu;
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
        _waitShiftValues[3] = LookupHonorWaitShift[base5Hash];
        _honorOr = LookupHonorOr[base5Hash] | (_meldLookupValues[3] & ~0b1_111_111_111_111L);
        _honorSum = LookupHonorSum[base5Hash] + _meldLookupValues[3];
      }
      else
      {
        _waitShiftValues[suitId] = LookupSuitWaitShift[base5Hash];
        var suitOr = LookupSuitOr[base5Hash] | _meldLookupValues[suitId];
        _suitOr[suitId] = suitOr + (1L << (BitIndex.ClosedIttsuu - 3));
        _fuFootprintOffsets[suitId] = (int) ((_waitShiftValues[suitId] >> 10) & 0b1111111111111L) * 680;
      }
    }
  }
}