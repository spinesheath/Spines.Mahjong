using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Score;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class ClassicScoreCalculator
  {
    private readonly IReadOnlyList<Tile> _concealedTiles;
    private readonly IReadOnlyList<State.Meld> _melds;
    private readonly TileType _winningTile;
    private readonly int _roundWind;
    private readonly int _seatWind;
    private readonly bool _isRon;

    private readonly Yaku _han;
    private readonly int _fu;
    private readonly bool _isClosed;
    private readonly IReadOnlyList<Arrangement> _arrangements;
    private readonly IReadOnlyList<Tile> _allTiles;
    private readonly IReadOnlyList<int> _allCounts;
    private readonly IReadOnlyList<int> _concealedCounts;
    private readonly int _suitPresence;

    private static readonly Dictionary<int, IReadOnlyList<Arrangement>>[] SuitArrangementCaches = 
    {
      new Dictionary<int, IReadOnlyList<Arrangement>>(),
      new Dictionary<int, IReadOnlyList<Arrangement>>(),
      new Dictionary<int, IReadOnlyList<Arrangement>>()
    };

    private ClassicScoreCalculator(IReadOnlyList<Tile> concealedTiles, IReadOnlyList<State.Meld> melds, TileType winningTile, int roundWind, int seatWind, bool isRon)
    {
      _concealedTiles = concealedTiles;
      _melds = melds;
      _winningTile = winningTile;
      _roundWind = roundWind;
      _seatWind = seatWind;
      _isRon = isRon;
      _isClosed = IsClosed();

      var allTiles = new List<Tile>();
      var allCounts = new int[34];
      var concealedCounts = new int[34];
      foreach (var tile in concealedTiles)
      {
        allTiles.Add(tile);
        var tileType = tile.TileType;
        _suitPresence |= 1 << tileType.SuitId;
        allCounts[tileType.TileTypeId] += 1;
        concealedCounts[tileType.TileTypeId] += 1;
      }

      foreach (var tile in melds.SelectMany(t => t.Tiles))
      {
        allTiles.Add(tile);
        var tileType = tile.TileType;
        _suitPresence |= 1 << tileType.SuitId;
        allCounts[tileType.TileTypeId] += 1;
      }

      _allTiles = allTiles;
      _allCounts = allCounts;
      _concealedCounts = concealedCounts;

      _arrangements = Arrangements();

      (_han, _fu) = Flags();
    }

    private bool IsClosed()
    {
      return _melds.All(m => m.IsKan && m.CalledTile == null);
    }

    private IReadOnlyList<Arrangement> Arrangements()
    {
      var result = new List<Arrangement>();

      if (_allCounts[32] == 1 || _allCounts[33] == 1)
      {
        var pair = _concealedTiles.GroupBy(t => t.TileType).First(g => g.Count() == 2).Key;
        result.Add(new Arrangement {IsKokushi = true, Pair = pair});
      }
      else
      {
        if (_allCounts.All(c => c == 0 || c == 2) && !_melds.Any())
        {
          result.Add(new Arrangement { IsChiitoitsu = true });
        }

        var manzu = CachedSuitArrangements(0);
        var pinzu = CachedSuitArrangements(1);
        var souzu = CachedSuitArrangements(2);
        var jihai = Jihai();

        foreach (var m in manzu)
        {
          foreach (var p in pinzu)
          {
            foreach (var s in souzu)
            {
              var pair = m.Pair ?? p.Pair ?? s.Pair ?? jihai.Pair;
              var a = new Arrangement {Pair = pair};
              a.Koutsus.AddRange(m.Koutsus.Concat(p.Koutsus).Concat(s.Koutsus).Concat(jihai.Koutsus));
              a.Shuntsus.AddRange(m.Shuntsus.Concat(p.Shuntsus).Concat(s.Shuntsus));

              if (a.Koutsus.Count + a.Shuntsus.Count + _melds.Count == 4 && a.Pair != null)
              {
                result.Add(a);
              }
            }
          }
        }
      }
      
      return result;
    }

    private IReadOnlyList<Arrangement> CachedSuitArrangements(int suitId)
    {
      var tileCounts = new int[9];
      var tileCount = 0;
      var hash = 0;
      foreach (var tile in _concealedTiles)
      {
        var tileType = tile.TileType;
        if (tileType.SuitId == suitId)
        {
          tileCounts[tileType.Index] += 1;
          tileCount += 1;
          hash += Base5Table[tileType.Index];
        }
      }

      if (hash > 0)
      {
        if (SuitArrangementCaches[suitId].TryGetValue(hash, out var result))
        {
          return result;
        }

        var newResult = SuitArrangements(suitId, tileCount, tileCounts).DefaultIfEmpty(new Arrangement()).ToList();
        SuitArrangementCaches[suitId][hash] = newResult;
        return newResult;
      }

      return SuitArrangements(suitId, tileCount, tileCounts).DefaultIfEmpty(new Arrangement()).ToList();
    }

    private protected static readonly int[] Base5Table =
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

    private Arrangement Jihai()
    {
      var arrangement = new Arrangement();
      for (var i = 27; i < 34; i++)
      {
        var tileType = TileType.FromTileTypeId(i);
        if (_concealedCounts[i] == 2)
        {
          arrangement.Pair = tileType;
        }

        if (_concealedCounts[i] == 3)
        {
          arrangement.Koutsus.Add(tileType);
        }
      }
      
      return arrangement;
    }

    private IEnumerable<Arrangement> SuitArrangements(int suitId, int tileCount, int[] tileCounts)
    {
      if (tileCount % 3 == 1)
      {
        yield break;
      }

      var hasPair = tileCount % 3 == 2;
      if (hasPair)
      {
        for (var i = 0; i < 9; i++)
        {
          if (tileCounts[i] > 1)
          {
            var counts = tileCounts.ToArray();
            counts[i] -= 2;

            var shuntsus = new List<int>();
            var koutsus = new List<int>();
            var mixedBlock = (int?)null;
            var badShape = false;

            for (var j = 0; j < 9; j++)
            {
              var count = counts[j];
              if (count == 0)
              {
                continue;
              }

              if (count < 3)
              {
                if (j > 6 || counts[j + 1] < count || counts[j + 2] < count)
                {
                  badShape = true;
                  break;
                }

                shuntsus.AddRange(Enumerable.Repeat(j, count));
                counts[j] -= count;
                counts[j + 1] -= count;
                counts[j + 2] -= count;
              }

              if (count == 4)
              {
                if (j > 6 || counts[j + 1] == 0 || counts[j + 2] == 0)
                {
                  badShape = true;
                  break;
                }

                shuntsus.Add(j);
                counts[j] -= 1;
                counts[j + 1] -= 1;
                counts[j + 2] -= 1;
                count -= 1;
              }

              if (count == 3)
              {
                if (j < 7 && counts[j + 1] >= 3 && counts[j + 2] >= 3)
                {
                  mixedBlock = j;
                  counts[j] -= 3;
                  counts[j + 1] -= 3;
                  counts[j + 2] -= 3;
                }
                else
                {
                  koutsus.Add(j);
                  counts[j] -= 3;
                }
              }
            }

            if (!badShape)
            {
              var arrangement = new Arrangement {Pair = TileType.FromTileTypeId(suitId * 9 + i)};
              arrangement.Koutsus.AddRange(koutsus.Select(t => TileType.FromTileTypeId(suitId * 9 + t)));
              arrangement.Shuntsus.AddRange(shuntsus.Select(t => TileType.FromTileTypeId(suitId * 9 + t)));

              if (mixedBlock == null)
              {
                yield return arrangement;
              }
              else
              {
                var mixedBlockTileType = TileType.FromTileTypeId(suitId * 9 + mixedBlock.Value);
                arrangement.Koutsus.Add(mixedBlockTileType);
                arrangement.Koutsus.Add(TileType.FromTileTypeId(suitId * 9 + mixedBlock.Value + 1));
                arrangement.Koutsus.Add(TileType.FromTileTypeId(suitId * 9 + mixedBlock.Value + 2));
                yield return arrangement;

                var arrangement2 = new Arrangement { Pair = TileType.FromTileTypeId(suitId * 9 + i) };
                arrangement2.Koutsus.AddRange(koutsus.Select(t => TileType.FromTileTypeId(suitId * 9 + t)));
                arrangement2.Shuntsus.AddRange(shuntsus.Select(t => TileType.FromTileTypeId(suitId * 9 + t)));
                arrangement2.Shuntsus.Add(mixedBlockTileType);
                arrangement2.Shuntsus.Add(mixedBlockTileType);
                arrangement2.Shuntsus.Add(mixedBlockTileType);
                yield return arrangement2;
              }
            }
          }
        }
      }
      else
      {
        var counts = tileCounts.ToArray();
        var shuntsus = new List<int>();
        var koutsus = new List<int>();
        var mixedBlock = (int?)null;
        var badShape = false;

        for (var j = 0; j < 9; j++)
        {
          var count = counts[j];
          if (count == 0)
          {
            continue;
          }

          if (count < 3)
          {
            if (j > 6 || counts[j + 1] < count || counts[j + 2] < count)
            {
              badShape = true;
              continue;
            }

            shuntsus.AddRange(Enumerable.Repeat(j, count));
            counts[j] -= count;
            counts[j + 1] -= count;
            counts[j + 2] -= count;
          }

          if (count == 4)
          {
            if (j > 6 || counts[j + 1] == 0 || counts[j + 2] == 0)
            {
              badShape = true;
              continue;
            }

            shuntsus.Add(j);
            counts[j] -= 1;
            counts[j + 1] -= 1;
            counts[j + 2] -= 1;
            count -= 1;
          }

          if (count == 3)
          {
            if (j < 7 && counts[j + 1] >= 3 && counts[j + 2] >= 3)
            {
              mixedBlock = j;
              counts[j] -= 3;
              counts[j + 1] -= 3;
              counts[j + 2] -= 3;
            }
            else
            {
              koutsus.Add(j);
              counts[j] -= 3;
            }
          }
        }

        if (!badShape)
        {
          var arrangement = new Arrangement();
          arrangement.Koutsus.AddRange(koutsus.Select(t => TileType.FromTileTypeId(suitId * 9 + t)));
          arrangement.Shuntsus.AddRange(shuntsus.Select(t => TileType.FromTileTypeId(suitId * 9 + t)));

          if (mixedBlock == null)
          {
            yield return arrangement;
          }
          else
          {
            var mixedBlockTileType = TileType.FromTileTypeId(suitId * 9 + mixedBlock.Value);
            arrangement.Koutsus.Add(mixedBlockTileType);
            arrangement.Koutsus.Add(TileType.FromTileTypeId(suitId * 9 + mixedBlock.Value + 1));
            arrangement.Koutsus.Add(TileType.FromTileTypeId(suitId * 9 + mixedBlock.Value + 2));
            yield return arrangement;

            var arrangement2 = new Arrangement();
            arrangement2.Koutsus.AddRange(koutsus.Select(t => TileType.FromTileTypeId(suitId * 9 + t)));
            arrangement2.Shuntsus.AddRange(shuntsus.Select(t => TileType.FromTileTypeId(suitId * 9 + t)));
            arrangement2.Shuntsus.Add(mixedBlockTileType);
            arrangement2.Shuntsus.Add(mixedBlockTileType);
            arrangement2.Shuntsus.Add(mixedBlockTileType);
            yield return arrangement2;
          }
        }
      }
    }

    private class Arrangement
    {
      public bool IsChiitoitsu { get; set; }

      public bool IsKokushi { get; set; }

      public TileType? Pair { get; set; }

      public List<TileType> Shuntsus { get; } = new List<TileType>();

      public List<TileType> Koutsus { get; } = new List<TileType>();
    }

    public static (Yaku, int) Ron(TileType winningTile, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds, IReadOnlyList<Tile> concealedTiles)
    {
      var c = new ClassicScoreCalculator(concealedTiles, melds, winningTile, roundWind, seatWind, true);
      return (c._han, c._fu);
    }

    public static (Yaku, int) Tsumo(TileType winningTile, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds, IReadOnlyList<Tile> concealedTiles)
    {
      var c = new ClassicScoreCalculator(concealedTiles, melds, winningTile, roundWind, seatWind, false);
      return (c._han, c._fu);
    }

    public static (Yaku, int) Chankan(TileType winningTile, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds, IReadOnlyList<Tile> concealedTiles)
    {
      var c = new ClassicScoreCalculator(concealedTiles, melds, winningTile, roundWind, seatWind, true);
      return (c._han, c._fu);
    }

    private (Yaku, int) Flags()
    {
      var bestHan = 0;
      var bestFu = 0;
      var results = new HashSet<Yaku>();
      foreach (var arrangement in _arrangements)
      {
        var (yaku, fu) = YakuForArrangement(arrangement);
        var han = Han(yaku);

        if (han > bestHan || han == bestHan && fu > bestFu)
        {
          results.Clear();
          bestHan = han;
          bestFu = fu;
        }

        if (han == bestHan && fu == bestFu)
        {
          results.Add(yaku);
        }
      }

      return (results.OrderByDescending(x => x).First(), bestFu);
    }

    private int Han(Yaku yaku)
    {
      if ((yaku & AllYakuman) != Yaku.None)
      {
        return int.MaxValue;
      }
      
      var setBits1 = (int)System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount((ulong)(yaku & HanMask1));
      var setBits2 = (int)System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount((ulong)(yaku & HanMask2));
      var setBits4 = (int)System.Runtime.Intrinsics.X86.Popcnt.X64.PopCount((ulong)(yaku & HanMask4));
      return setBits1 + 2 * setBits2 + 4 * setBits4;
    }

    private (Yaku, int) YakuForArrangement(Arrangement arrangement)
    {
      var result = Yaku.None;

      result |= MenzenTsumo();
      result |= Pinfu(arrangement);
      result |= Tanyao();
      result |= JikazeTon();
      result |= JikazeNan();
      result |= JikazeShaa();
      result |= JikazePei();
      result |= BakazeTon();
      result |= BakazeNan();
      result |= BakazeShaa();
      result |= BakazePei();
      result |= Haku();
      result |= Hatsu();
      result |= Chun();
      result |= Chiitoitsu(arrangement);
      result |= ChantaJunchan(arrangement);
      result |= Ittsuu(arrangement);
      result |= SanshokuDoujun(arrangement);
      result |= SanshokuDoukou(arrangement);
      result |= Sankantsu();
      result |= Toitoi(arrangement);
      result |= SanankouSuuankou(arrangement);
      result |= ShousangenDaisangen(arrangement);
      result |= HonroutouChinroutouTsuuiisou();
      result |= IipeikouRyanpeikou(arrangement);
      result |= HonitsuChinitsu();
      result |= Ryuuiisou();
      result |= ChuurenPoutou(arrangement);
      result |= Kokushi(arrangement);
      result |= Suushiihou(arrangement);
      result |= Suukantsu();

      //{ ScoringFieldYaku.None, Yaku.Riichi },
      //{ ScoringFieldYaku.None, Yaku.Ippatsu },
      //{ ScoringFieldYaku.None, Yaku.Chankan },
      //{ ScoringFieldYaku.None, Yaku.RinshanKaihou },
      //{ ScoringFieldYaku.None, Yaku.HaiteiRaoyue },
      //{ ScoringFieldYaku.None, Yaku.HouteiRaoyui },

      //{ ScoringFieldYaku.None, Yaku.DoubleRiichi },

      //{ ScoringFieldYaku.None, Yaku.Renhou },
      //{ ScoringFieldYaku.None, Yaku.Tenhou },
      //{ ScoringFieldYaku.None, Yaku.Chiihou },

      //{ ScoringFieldYaku.None, Yaku.Dora },
      //{ ScoringFieldYaku.None, Yaku.UraDora },
      //{ ScoringFieldYaku.None, Yaku.AkaDora },

      var fu = Fu(arrangement, result);

      if ((result & AllYakuman) != Yaku.None)
      {
        return (result & AllYakuman, fu);
      }
      
      return (result, fu);
    }

    private int Fu(Arrangement arrangement, Yaku yaku)
    {
      if (arrangement.IsKokushi)
      {
        return 0;
      }

      if ((yaku & Yaku.Chiitoitsu) != 0)
      {
        return 25;
      }

      var fu = 20;
      
      foreach (var meld in _melds)
      {
        var isKyuuhai = meld.LowestTile.TileType.IsKyuuhai;
        switch (meld.MeldType)
        {
          case MeldType.Koutsu:
            fu += isKyuuhai ? 4 : 2;
            break;
          case MeldType.AddedKan:
          case MeldType.CalledKan:
            fu += isKyuuhai ? 16 : 8;
            break;
          case MeldType.ClosedKan:
            fu += isKyuuhai ? 32 : 16;
            break;
        }
      }

      foreach (var koutsu in arrangement.Koutsus)
      {
        if (!_isRon || koutsu != _winningTile || HasShuntsuWithWinningTile(arrangement))
        {
          fu += koutsu.IsKyuuhai ? 8 : 4;
        }
        else
        {
          fu += koutsu.IsKyuuhai ? 4 : 2;
        }
      }

      if (_isClosed && _isRon)
      {
        fu += 10;
      }

      var isPinfu = (yaku & Yaku.Pinfu) != 0;
      if (!_isRon && !isPinfu)
      {
        fu += 2;
      }

      if (arrangement.Pair!.TileTypeId > 30)
      {
        fu += 2;
      }

      if (arrangement.Pair.TileTypeId == 27 + _roundWind)
      {
        fu += 2;
      }

      if (arrangement.Pair.TileTypeId == 27 + _seatWind)
      {
        fu += 2;
      }

      if (!isPinfu)
      {
        if (arrangement.Pair == _winningTile)
        {
          fu += 2;
        }
        else if (arrangement.Shuntsus.Any(s => s.TileTypeId == _winningTile.TileTypeId - 1))
        {
          fu += 2;
        }
        else if (arrangement.Shuntsus.Any(s => s.Index == 0 && s.TileTypeId == _winningTile.TileTypeId - 2))
        {
          fu += 2;
        }
        else if (arrangement.Shuntsus.Any(s => s.Index == 6 && s == _winningTile))
        {
          fu += 2;
        }
      }

      if (!_isClosed && fu == 20)
      {
        return 30;
      }

      fu += fu % 10 == 0 ? 0 : 10 - fu % 10;
      return fu;
    }

    private Yaku Ryuuiisou()
    {
      if (_allTiles.All(IsRyuuiisouTile))
      {
        return Yaku.Ryuuiisou;
      }

      return Yaku.None;
    }

    private bool IsRyuuiisouTile(Tile tile)
    {
      var tileType = tile.TileType;
      if (tileType.Suit == Suit.Jihai && tileType.Index == 5)
      {
        return true;
      }

      if (tileType.Suit == Suit.Souzu && (tileType.Index % 2 == 1 || tileType.Index == 2))
      {
        return true;
      }

      return false;
    }

    private Yaku ChuurenPoutou(Arrangement arrangement)
    {
      if (arrangement.IsKokushi || arrangement.IsChiitoitsu || _melds.Any())
      {
        return Yaku.None;
      }

      if (_suitPresence == 1 || _suitPresence == 2 || _suitPresence == 4)
      {
        var counts = new int[9];
        foreach (var tile in _allTiles)
        {
          counts[tile.TileType.Index] += 1;
        }

        var center = counts[1] * counts[2] * counts[3] * counts[4] * counts[5] * counts[6] * counts[7];
        if (counts[0] >= 3 && counts[8] >= 3 && center > 0 && center <= 2)
        {
          if (counts[_winningTile.Index] == 2 || counts[_winningTile.Index] == 4)
          {
            return Yaku.JunseiChuurenPoutou;
          }

          return Yaku.ChuurenPoutou;
        }
      }

      return Yaku.None;
    }

    private Yaku Suushiihou(Arrangement arrangement)
    {
      if (arrangement.IsKokushi || arrangement.IsChiitoitsu)
      {
        return Yaku.None;
      }

      var concealed = arrangement.Koutsus.Count(k => k.Suit == Suit.Jihai && k.Index < 4);
      var melded = _melds.Count(m => m.LowestTile.TileType.Suit == Suit.Jihai && m.LowestTile.TileType.Index < 4);
      var sum = concealed + melded;
      var pair = arrangement.Pair!.Suit == Suit.Jihai && arrangement.Pair.Index < 4;

      if (sum == 4)
      {
        return Yaku.Daisuushii;
      }

      if (sum == 3 && pair)
      {
        return Yaku.Shousuushii;
      }

      return Yaku.None;
    }

    private Yaku Kokushi(Arrangement arrangement)
    {
      if (arrangement.IsKokushi)
      {
        return arrangement.Pair == _winningTile ? Yaku.KokushiMusouJuusanMen : Yaku.KokushiMusou;
      }

      return Yaku.None;
    }

    private Yaku HonitsuChinitsu()
    {
      if (_suitPresence == 4 || _suitPresence == 2 || _suitPresence == 1)
      {
        return _isClosed ? Yaku.ClosedChinitsu : Yaku.OpenChinitsu;
      }

      if (_suitPresence == 9 || _suitPresence == 10 || _suitPresence == 12)
      {
        return _isClosed ? Yaku.ClosedHonitsu : Yaku.OpenHonitsu;
      }

      return Yaku.None;
    }

    private Yaku HonroutouChinroutouTsuuiisou()
    {
      if (_allTiles.Any(t => !t.TileType.IsKyuuhai))
      {
        return Yaku.None;
      }

      if (_allTiles.All(t => t.TileType.Suit == Suit.Jihai))
      {
        return Yaku.Tsuuiisou;
      }

      if (_allTiles.All(t => t.TileType.Suit != Suit.Jihai))
      {
        return Yaku.Chinroutou;
      }

      return Yaku.Honroutou;
    }

    private Yaku ShousangenDaisangen(Arrangement arrangement)
    {
      if (arrangement.IsKokushi || arrangement.IsChiitoitsu)
      {
        return Yaku.None;
      }

      var melded = _melds.Count(m => m.LowestTile.TileType.Suit == Suit.Jihai && m.LowestTile.TileType.Index > 3);
      var closed = arrangement.Koutsus.Count(k => k.Suit == Suit.Jihai && k.Index > 3);
      var hasDragonPair = arrangement.Pair!.Suit == Suit.Jihai && arrangement.Pair.Index > 3;
      if (hasDragonPair && melded + closed == 2)
      {
        return Yaku.Shousangen;
      }

      if (melded + closed == 3)
      {
        return Yaku.Daisangen;
      }

      return Yaku.None;
    }

    private Yaku SanankouSuuankou(Arrangement arrangement)
    {
      var ankanCount = _melds.Count(m => m.IsKan && m.CalledTile == null);
      var considerWinningTile = _isRon && !HasShuntsuWithWinningTile(arrangement);
      var ankouCount = arrangement.Koutsus.Count(k => !considerWinningTile || k != _winningTile);

      var sum = ankanCount + ankouCount;
      if (sum == 3)
      {
        return Yaku.Sanankou;
      }

      if (sum == 4)
      {
        return arrangement.Pair == _winningTile ? Yaku.SuuankouTanki : Yaku.Suuankou;
      }

      return Yaku.None;
    }

    private bool HasShuntsuWithWinningTile(Arrangement arrangement)
    {
      return arrangement.Shuntsus.Any(s => s.Suit == _winningTile.Suit && s.Index <= _winningTile.Index && s.Index + 2 >= _winningTile.Index);
    }

    private Yaku Toitoi(Arrangement arrangement)
    {
      if (arrangement.IsChiitoitsu || arrangement.IsKokushi || _melds.Any(m => m.MeldType == MeldType.Shuntsu) || arrangement.Shuntsus.Any())
      {
        return Yaku.None;
      }

      return Yaku.Toitoihou;
    }

    private Yaku Sankantsu()
    {
      return _melds.Count(m => m.IsKan) == 3 ? Yaku.Sankantsu : Yaku.None;
    }

    private Yaku SanshokuDoukou(Arrangement arrangement)
    {
      var suits = new int[4];
      foreach (var meld in _melds)
      {
        if (meld.MeldType == MeldType.Koutsu || meld.IsKan)
        {
          var tileType = meld.LowestTile.TileType;
          suits[tileType.SuitId] |= 1 << tileType.Index;
        }
      }

      foreach (var koutsu in arrangement.Koutsus)
      {
        suits[koutsu.SuitId] |= 1 << koutsu.Index;
      }

      if ((suits[0] & suits[1] & suits[2]) != 0)
      {
        return Yaku.SanshokuDoukou;
      }

      return Yaku.None;
    }

    private Yaku SanshokuDoujun(Arrangement arrangement)
    {
      var suits = new int[3];
      foreach (var meld in _melds)
      {
        if (meld.MeldType == MeldType.Shuntsu)
        {
          var tileType = meld.LowestTile.TileType;
          suits[tileType.SuitId] |= 1 << tileType.Index;
        }
      }

      foreach (var shuntsu in arrangement.Shuntsus)
      {
        suits[shuntsu.SuitId] |= 1 << shuntsu.Index;
      }

      if ((suits[0] & suits[1] & suits[2]) != 0)
      {
        return _isClosed ? Yaku.ClosedSanshokuDoujun : Yaku.OpenSanshokuDoujun;
      }

      return Yaku.None;
    }

    private Yaku Ittsuu(Arrangement arrangement)
    {
      var suits = new int[3];
      foreach (var meld in _melds)
      {
        if (meld.MeldType == MeldType.Shuntsu)
        {
          var tileType = meld.LowestTile.TileType;
          suits[tileType.SuitId] |= 1 << tileType.Index;
        }
      }

      foreach (var shuntsu in arrangement.Shuntsus)
      {
        suits[shuntsu.SuitId] |= 1 << shuntsu.Index;
      }

      if (suits.Any(s => (s & 0b1001001) == 0b1001001))
      {
        return _isClosed ? Yaku.ClosedIttsuu : Yaku.OpenIttsuu;
      }

      return Yaku.None;
    }

    private Yaku ChantaJunchan(Arrangement arrangement)
    {
      if (arrangement.IsChiitoitsu || arrangement.IsKokushi)
      {
        return Yaku.None;
      }

      if (_melds.Any(m => !ContainsKyuuhai(m)) || !arrangement.Pair!.IsKyuuhai || arrangement.Koutsus.Any(k => !k.IsKyuuhai))
      {
        return Yaku.None;
      }

      if (arrangement.Shuntsus.Any(s => s.Index != 0 && s.Index != 6))
      {
        return Yaku.None;
      }

      if (_melds.All(m => m.MeldType != MeldType.Shuntsu) && !arrangement.Shuntsus.Any())
      {
        return Yaku.None;
      }

      if (_allTiles.All(t => t.TileType.Suit != Suit.Jihai))
      {
        return _isClosed ? Yaku.ClosedJunchan : Yaku.OpenJunchan;
      }

      return _isClosed ? Yaku.ClosedChanta : Yaku.OpenChanta;
    }

    private static bool ContainsKyuuhai(State.Meld meld)
    {
      return meld.Tiles.Any(t => t.TileType.IsKyuuhai);
    }
    
    private Yaku Chiitoitsu(Arrangement arrangement)
    {
      return arrangement.IsChiitoitsu ? Yaku.Chiitoitsu : Yaku.None;
    }

    private Yaku IipeikouRyanpeikou(Arrangement arrangement)
    {
      if (arrangement.IsChiitoitsu || arrangement.IsKokushi || !_isClosed)
      {
        return Yaku.None;
      }

      var count = 0;
      var grouped = arrangement.Shuntsus.GroupBy(s => s);
      foreach (var g in grouped)
      {
        count += g.Count() / 2;
      }

      if (count == 1)
      {
        return Yaku.Iipeikou;
      }

      return count == 2 ? Yaku.Ryanpeikou : Yaku.None;
    }

    private Yaku Pinfu(Arrangement arrangement)
    {
      if (arrangement.IsChiitoitsu || arrangement.IsKokushi || !_isClosed || arrangement.Shuntsus.Count != 4)
      {
        return Yaku.None;
      }

      var pair = arrangement.Pair;
      if (pair!.Suit == Suit.Jihai && (pair.Index > 3 || pair.Index == _roundWind || pair.Index == _seatWind))
      {
        return Yaku.None;
      }

      return arrangement.Shuntsus.Any(s => IsPinfuWait(s, _winningTile)) ? Yaku.Pinfu : Yaku.None;
    }

    private static bool IsPinfuWait(TileType shuntsu, TileType wait)
    {
      return shuntsu == wait && shuntsu.Index < 6 || shuntsu.Suit == wait.Suit && shuntsu.Index + 2 == wait.Index && wait.Index > 2;
    }

    private Yaku Haku()
    {
      return HasHonorTriplet(TileType.FromSuitAndIndex(Suit.Jihai, 4)) ? Yaku.Haku : Yaku.None;
    }

    private Yaku Hatsu()
    {
      return HasHonorTriplet(TileType.FromSuitAndIndex(Suit.Jihai, 5)) ? Yaku.Hatsu : Yaku.None;
    }

    private Yaku Chun()
    {
      return HasHonorTriplet(TileType.FromSuitAndIndex(Suit.Jihai, 6)) ? Yaku.Chun : Yaku.None;
    }

    private Yaku BakazeTon()
    {
      return _roundWind == 0 && HasHonorTriplet(TileType.Ton) ? Yaku.BakazeTon : Yaku.None;
    }

    private Yaku BakazeNan()
    {
      return _roundWind == 1 && HasHonorTriplet(TileType.Nan) ? Yaku.BakazeNan : Yaku.None;
    }

    private Yaku BakazeShaa()
    {
      return _roundWind == 2 && HasHonorTriplet(TileType.Shaa) ? Yaku.BakazeShaa : Yaku.None;
    }

    private Yaku BakazePei()
    {
      return _roundWind == 3 && HasHonorTriplet(TileType.Pei) ? Yaku.BakazePei : Yaku.None;
    }

    private Yaku JikazeTon()
    {
      return _seatWind == 0 && HasHonorTriplet(TileType.Ton) ? Yaku.JikazeTon : Yaku.None;
    }

    private Yaku JikazeNan()
    {
      return _seatWind == 1 && HasHonorTriplet(TileType.Nan) ? Yaku.JikazeNan : Yaku.None;
    }

    private Yaku JikazeShaa()
    {
      return _seatWind == 2 && HasHonorTriplet(TileType.Shaa) ? Yaku.JikazeShaa : Yaku.None;
    }

    private Yaku JikazePei()
    {
      return _seatWind == 3 && HasHonorTriplet(TileType.Pei) ? Yaku.JikazePei : Yaku.None;
    }

    private bool HasHonorTriplet(TileType tileType)
    {
      return _allCounts[tileType.TileTypeId] >= 3;
    }

    private Yaku Suukantsu()
    {
      return _melds.Count(m => m.IsKan) == 4 ? Yaku.Suukantsu : Yaku.None;
    }

    private Yaku Tanyao()
    {
      if (_allTiles.Any(t => t.TileType.IsKyuuhai))
      {
        return Yaku.None;
      }

      return _isClosed ? Yaku.ClosedTanyao : Yaku.OpenTanyao;
    }

    private Yaku MenzenTsumo()
    {
      return !_isRon && _melds.All(IsAnkan) ? Yaku.MenzenTsumo : Yaku.None;
    }

    private static bool IsAnkan(State.Meld m)
    {
      return m.IsKan && m.CalledTile == null;
    }

    private static readonly Yaku AllYakuman =
      Yaku.ChuurenPoutou |
      Yaku.JunseiChuurenPoutou |
      Yaku.Ryuuiisou |
      Yaku.Shousuushii |
      Yaku.Daisuushii |
      Yaku.Chiihou |
      Yaku.Chinroutou |
      Yaku.Suuankou |
      Yaku.SuuankouTanki |
      Yaku.KokushiMusou |
      Yaku.KokushiMusouJuusanMen |
      Yaku.Tsuuiisou |
      Yaku.Daisangen;

    private static readonly Yaku HanMask1 =
      Yaku.MenzenTsumo |
      Yaku.Riichi |
      Yaku.Ippatsu |
      Yaku.Chankan |
      Yaku.RinshanKaihou |
      Yaku.HaiteiRaoyue |
      Yaku.HouteiRaoyui |
      Yaku.Pinfu |
      Yaku.OpenTanyao |
      Yaku.ClosedTanyao |
      Yaku.Iipeikou |
      Yaku.JikazeTon |
      Yaku.JikazeShaa |
      Yaku.JikazeNan |
      Yaku.JikazePei |
      Yaku.BakazeTon |
      Yaku.BakazeShaa |
      Yaku.BakazeNan |
      Yaku.BakazePei |
      Yaku.Haku |
      Yaku.Hatsu |
      Yaku.Chun |
      Yaku.OpenChanta |
      Yaku.OpenIttsuu |
      Yaku.OpenSanshokuDoujun |
      
      Yaku.Ryanpeikou |
      Yaku.ClosedJunchan |
      Yaku.ClosedHonitsu |

      Yaku.OpenChinitsu |

      Yaku.Renhou |
      Yaku.Tenhou |
      Yaku.Chiihou |
      Yaku.Daisangen |
      Yaku.Suuankou |
      Yaku.SuuankouTanki |
      Yaku.Tsuuiisou |
      Yaku.Ryuuiisou |
      Yaku.Chinroutou |
      Yaku.ChuurenPoutou |
      Yaku.JunseiChuurenPoutou |
      Yaku.KokushiMusou |
      Yaku.KokushiMusouJuusanMen |
      Yaku.Daisuushii |
      Yaku.Shousuushii |
      Yaku.Suukantsu;

    private static readonly Yaku HanMask2 =
      Yaku.DoubleRiichi |
      Yaku.Chiitoitsu |
      Yaku.ClosedChanta |
      Yaku.ClosedIttsuu |
      Yaku.ClosedSanshokuDoujun |
      Yaku.SanshokuDoukou |
      Yaku.Sankantsu |
      Yaku.Toitoihou |
      Yaku.Sanankou |
      Yaku.Shousangen |
      Yaku.Honroutou |

      Yaku.Ryanpeikou |
      Yaku.OpenJunchan |
      Yaku.ClosedJunchan |
      Yaku.OpenHonitsu |
      Yaku.ClosedHonitsu |

      Yaku.ClosedChinitsu;

    private static readonly Yaku HanMask4 =
      Yaku.OpenChinitsu |
      Yaku.ClosedChinitsu;
  }
}