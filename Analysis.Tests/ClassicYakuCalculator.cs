using System.Collections.Generic;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.Score;

namespace Spines.Mahjong.Analysis.Tests
{
  internal class ClassicYakuCalculator
  {
    private readonly IReadOnlyList<Tile> _concealedTiles;
    private readonly IReadOnlyList<State.Meld> _melds;
    private readonly Tile _winningTile;
    private readonly int _roundWind;
    private readonly int _seatWind;
    private readonly bool _isRon;

    private readonly Yaku _result;
    private readonly bool _isClosed;
    private readonly IReadOnlyList<Arrangement> _arrangements;
    private readonly IReadOnlyList<Tile> _allTiles;

    private ClassicYakuCalculator(IReadOnlyList<Tile> concealedTiles, IReadOnlyList<State.Meld> melds, Tile winningTile, int roundWind, int seatWind, bool isRon)
    {
      _concealedTiles = concealedTiles;
      _melds = melds;
      _winningTile = winningTile;
      _roundWind = roundWind;
      _seatWind = seatWind;
      _isRon = isRon;
      _isClosed = IsClosed();

      _arrangements = Arrangements();
      _allTiles = concealedTiles.Concat(_melds.SelectMany(m => m.Tiles)).ToList();

      _result = Flags();
    }

    private bool IsClosed()
    {
      return _melds.All(m => m.IsKan && m.CalledTile == null);
    }

    private IReadOnlyList<Arrangement> Arrangements()
    {
      var result = new List<Arrangement>();

      var groupedByType = _concealedTiles.GroupBy(t => t.TileType).ToList();
      if (groupedByType.Any(g => g.Key.Suit == Suit.Jihai && g.Count() == 1))
      {
        var pair = groupedByType.First(g => g.Count() == 2).Key;
        result.Add(new Arrangement {IsKokushi = true, Pair = pair});
      }
      else
      {
        if (groupedByType.All(g => g.Count() == 2) && !_melds.Any())
        {
          result.Add(new Arrangement { IsChiitoitsu = true });
        }

        var manzu = SuitArrangements(0).DefaultIfEmpty(new Arrangement()).ToList();
        var pinzu = SuitArrangements(1).DefaultIfEmpty(new Arrangement()).ToList();
        var souzu = SuitArrangements(2).DefaultIfEmpty(new Arrangement()).ToList();
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

    private Arrangement Jihai()
    {
      var tileCounts = new int[7];
      foreach (var tile in _concealedTiles)
      {
        var tileType = tile.TileType;
        if (tileType.Suit == Suit.Jihai)
        {
          tileCounts[tileType.Index] += 1;
        }
      }

      var arrangement = new Arrangement();
      for (var i = 0; i < 7; i++)
      {
        if (tileCounts[i] == 2)
        {
          arrangement.Pair = TileType.FromSuitAndIndex(Suit.Jihai, i);
        }

        if (tileCounts[i] == 3)
        {
          arrangement.Koutsus.Add(TileType.FromSuitAndIndex(Suit.Jihai, i));
        }
      }
      
      return arrangement;
    }

    private IEnumerable<Arrangement> SuitArrangements(int suitId)
    {
      var tileCounts = new int[9];
      var tileCount = 0;
      foreach (var tile in _concealedTiles)
      {
        var tileType = tile.TileType;
        if (tileType.SuitId == suitId)
        {
          tileCounts[tileType.Index] += 1;
          tileCount += 1;
        }
      }

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

    public static Yaku Ron(Tile winningTile, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds, IReadOnlyList<Tile> concealedTiles)
    {
      return new ClassicYakuCalculator(concealedTiles, melds, winningTile, roundWind, seatWind, true)._result;
    }

    public static Yaku Tsumo(Tile winningTile, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds, IReadOnlyList<Tile> concealedTiles)
    {
      return new ClassicYakuCalculator(concealedTiles, melds, winningTile, roundWind, seatWind, false)._result;
    }

    public static Yaku Chankan(Tile winningTile, int roundWind, int seatWind, IReadOnlyList<State.Meld> melds, IReadOnlyList<Tile> concealedTiles)
    {
      return new ClassicYakuCalculator(concealedTiles, melds, winningTile, roundWind, seatWind, true)._result;
    }

    private Yaku Flags()
    {
      var bestHan = 0;
      var results = new List<Yaku>();
      foreach (var arrangement in _arrangements)
      {
        var yaku = YakuForArrangement(arrangement);
        var han = Han(yaku);

        if (han > bestHan)
        {
          results.Clear();
          bestHan = han;
        }

        if (han == bestHan)
        {
          results.Add(yaku);
        }
      }
      
      return results.OrderByDescending(x => x).First();
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

    private Yaku YakuForArrangement(Arrangement arrangement)
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

      if ((result & AllYakuman) != Yaku.None)
      {
        return result & AllYakuman;
      }
      
      return result;
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

      var bySuit = _allTiles.GroupBy(t => t.TileType.Suit).ToList();
      if (bySuit.Count == 1 && bySuit.First().Key != Suit.Jihai)
      {
        var counts = new int[9];
        foreach (var tile in _allTiles)
        {
          counts[tile.TileType.Index] += 1;
        }

        var center = counts[1] * counts[2] * counts[3] * counts[4] * counts[5] * counts[6] * counts[7];
        if (counts[0] >= 3 && counts[8] >= 3 && center > 0 && center <= 2)
        {
          if (counts[_winningTile.TileType.Index] == 2 || counts[_winningTile.TileType.Index] == 4)
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
        return arrangement.Pair == _winningTile.TileType ? Yaku.KokushiMusouJuusanMen : Yaku.KokushiMusou;
      }

      return Yaku.None;
    }

    private Yaku HonitsuChinitsu()
    {
      var suitCount = _allTiles.GroupBy(t => t.TileType.SuitId).Count(g => g.Any());
      var honors = _allTiles.Any(t => t.TileType.Suit == Suit.Jihai);

      if (suitCount == 1 && !honors)
      {
        return _isClosed ? Yaku.ClosedChinitsu : Yaku.OpenChinitsu;
      }

      if (suitCount == 2 && honors)
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
      var tileType = _winningTile.TileType;
      var considerWinningTile = _isRon && !arrangement.Shuntsus.Any(s => s.Suit == tileType.Suit && s.Index <= tileType.Index && s.Index + 2 >= tileType.Index);
      var ankouCount = arrangement.Koutsus.Count(k => !considerWinningTile || k != tileType);

      var sum = ankanCount + ankouCount;
      if (sum == 3)
      {
        return Yaku.Sanankou;
      }

      if (sum == 4)
      {
        return arrangement.Pair == tileType ? Yaku.SuuankouTanki : Yaku.Suuankou;
      }

      return Yaku.None;
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

      var count = arrangement.Shuntsus.GroupBy(s => s).Count(g => g.Count() == 2 || g.Count() == 3) + 2 * arrangement.Shuntsus.GroupBy(s => s).Count(g => g.Count() == 4);
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

      var tileType = _winningTile.TileType;
      return arrangement.Shuntsus.Any(s => IsPinfuWait(s, tileType)) ? Yaku.Pinfu : Yaku.None;
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
      var concealed = _concealedTiles.Count(t => t.TileType == tileType) == 3;
      var melded = _melds.Any(m => m.LowestTile.TileType.Index == tileType.Index && m.LowestTile.TileType.Suit == tileType.Suit);
      return concealed || melded;
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