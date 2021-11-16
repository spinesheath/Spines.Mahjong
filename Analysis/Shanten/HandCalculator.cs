using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Spines.Mahjong.Analysis.Score;

namespace Spines.Mahjong.Analysis.Shanten
{
  /// <summary>
  /// Maintains tile counts and calculates the Shanten of a hand.
  /// TODO can potentially improve performance by rolling back in case of tsumogiri?
  /// </summary>
  public class HandCalculator : IHandCalculator
  {
    public HandCalculator(ShorthandParser initial)
      : this(initial.Tiles, initial.ManzuMeldIds, initial.PinzuMeldIds, initial.SouzuMeldIds, initial.JihaiMeldIds)
    {
    }

    public HandCalculator(IEnumerable<TileType> concealedTiles, IEnumerable<int> manzuMeldIds, IEnumerable<int> pinzuMeldIds, IEnumerable<int> souzuMeldIds, IEnumerable<int> jihaiMeldIds)
      : this()
    {
      InitializeSuitMelds(manzuMeldIds, 0);
      InitializeSuitMelds(pinzuMeldIds, 1);
      InitializeSuitMelds(souzuMeldIds, 2);
      InitializeJihaiMelds(jihaiMeldIds);
      Init(concealedTiles);
    }

    /// <summary>
    /// Creates a new instance of Hand.
    /// </summary>
    public HandCalculator()
    {
      // Don't need to initialize _arrangementValues here because the value for an empty hand is 0.
      // Don't need to set the melds in the suit classifiers here because entry state for concealed suits for a hand without melds is 0.
    }

    public IScoringData ScoringData => _scoringData;

    public override string ToString()
    {
      return Shanten + ": " + GetConcealedString(0, 'm') + GetConcealedString(1, 'p') + GetConcealedString(2, 's') +
             GetConcealedString(3, 'z') +
             GetMeldString(0, 'M') + GetMeldString(1, 'P') + GetMeldString(2, 'S') + GetHonorMeldString();
    }

    public int Shanten => CalculateShanten(ArrangementValues) - 1;

    public int ChiitoitsuShanten => Chiitoi.Shanten - 1;

    public int KokushiShanten => Kokushi.Shanten - 1;

    public int NormalHandShanten => ArrangementClassifier.Classify(ArrangementValues) - 1;

    public void Init(IEnumerable<TileType> tiles)
    {
      foreach (var tileType in tiles)
      {
        InHandByType[tileType.TileTypeId] += 1;
        Base5Hashes[tileType.SuitId] += Base5.Table[tileType.Index];

        var previousTileCount = ConcealedTiles[tileType.TileTypeId]++;
        Kokushi.Draw(tileType.KyuuhaiValue, previousTileCount);
        Chiitoi.Draw(previousTileCount);

        if (tileType.SuitId == 3)
        {
          ArrangementValues[3] = HonorClassifier.Draw(previousTileCount, (JihaiMeldBit >> tileType.Index) & 1);
        }
      }

      UpdateValue(0);
      UpdateValue(1);
      UpdateValue(2);

      _scoringData.Init(Base5Hashes);
    }

    public void Draw(TileType tileType)
    {
      Debug.Assert(TilesInHand() == 13, "Can only draw with a 13 tile hand.");
      Debug.Assert(InHandByType[tileType.TileTypeId] < 4, "Can't draw a tile with 4 of that tile in hand.");

      InHandByType[tileType.TileTypeId] += 1;
      Base5Hashes[tileType.SuitId] += Base5.Table[tileType.Index];

      var previousTileCount = ConcealedTiles[tileType.TileTypeId]++;
      Kokushi.Draw(tileType.KyuuhaiValue, previousTileCount);
      Chiitoi.Draw(previousTileCount);

      if (tileType.SuitId == 3)
      {
        ArrangementValues[3] = HonorClassifier.Draw(previousTileCount, (JihaiMeldBit >> tileType.Index) & 1);
      }
      else
      {
        UpdateValue(tileType.SuitId);
      }

      _scoringData.Draw(tileType.SuitId, Base5Hashes[tileType.SuitId]);
    }

    public void Discard(TileType tileType)
    {
      Debug.Assert(TilesInHand() == 14, "Can't discard from hand with less than 13 tiles.");
      Debug.Assert(InHandByType[tileType.TileTypeId] > 0, "Can't discard a tile that is not in the hand.");

      InHandByType[tileType.TileTypeId] -= 1;
      Base5Hashes[tileType.SuitId] -= Base5.Table[tileType.Index];

      var tileCountAfterDiscard = --ConcealedTiles[tileType.TileTypeId];
      Kokushi.Discard(tileType.KyuuhaiValue, tileCountAfterDiscard);
      Chiitoi.Discard(tileCountAfterDiscard);

      if (tileType.SuitId == 3)
      {
        ArrangementValues[3] = HonorClassifier.Discard(tileCountAfterDiscard, (JihaiMeldBit >> tileType.Index) & 1);
      }
      else
      {
        UpdateValue(tileType.SuitId);
      }

      _scoringData.Discard(tileType.SuitId, Base5Hashes[tileType.SuitId]);
    }

    public void Chii(TileType lowestTileType, TileType calledTileType)
    {
      Debug.Assert(TilesInHand() == 13, "chii only after discard");
      Debug.Assert(lowestTileType.Suit != Suit.Jihai, "Not a valid suit for chii");

      var suitId = lowestTileType.SuitId;
      var lowestIndex = lowestTileType.Index;

      var lowestId = lowestTileType.TileTypeId;
      ConcealedTiles[lowestId] -= 1;
      ConcealedTiles[lowestId + 1] -= 1;
      ConcealedTiles[lowestId + 2] -= 1;
      ConcealedTiles[calledTileType.TileTypeId] += 1;
      Base5Hashes[suitId] -= Base5.Table[lowestIndex] + Base5.Table[lowestIndex + 1] + Base5.Table[lowestIndex + 2] - Base5.Table[calledTileType.Index];

      _meldCount += 1;
      _melds[suitId] <<= 6;
      _melds[suitId] += 1 + lowestIndex;
      InHandByType[calledTileType.TileTypeId] += 1;
      SuitClassifiers[suitId].SetMelds(_melds[suitId]);
      UpdateValue(suitId);

      _scoringData.Chii(lowestTileType, Base5Hashes[suitId]);
    }

    public void Pon(TileType tileType)
    {
      Debug.Assert(TilesInHand() == 13, "pon only after discard");

      var suitId = tileType.SuitId;
      var index = tileType.Index;
      InHandByType[tileType.TileTypeId] += 1;
      var previousTiles = ConcealedTiles[tileType.TileTypeId];
      ConcealedTiles[tileType.TileTypeId] -= 2;
      Base5Hashes[suitId] -= 2 * Base5.Table[index];
      _meldCount += 1;
      _melds[suitId] <<= 6;
      _melds[suitId] += 1 + 7 + index;

      if (suitId < 3)
      {
        SuitClassifiers[suitId].SetMelds(_melds[suitId]);
        UpdateValue(suitId);
      }
      else
      {
        ArrangementValues[3] = HonorClassifier.Pon(previousTiles);
        JihaiMeldBit += 1 << index;
      }

      _scoringData.Pon(tileType, Base5Hashes[suitId]);
    }

    public void Shouminkan(TileType tileType)
    {
      Debug.Assert(TilesInHand() == 14, "shouminkan only after draw");

      var suitId = tileType.SuitId;
      ConcealedTiles[tileType.TileTypeId] -= 1;
      Base5Hashes[suitId] -= Base5.Table[tileType.Index];

      for (var i = 0; i < 4; i++)
      {
        var pon = 1 + 7 + tileType.Index;
        if (((_melds[suitId] >> (6 * i)) & 0b111111) == pon)
        {
          _melds[suitId] += 9 << (6 * i);
          break;
        }
      }

      if (suitId < 3)
      {
        SuitClassifiers[suitId].SetMelds(_melds[suitId]);
        UpdateValue(suitId);
      }
      else
      {
        ArrangementValues[3] = HonorClassifier.Shouminkan();
      }

      _scoringData.Shouminkan(tileType, Base5Hashes[suitId]);
    }

    public void Ankan(TileType tileType)
    {
      Debug.Assert(TilesInHand() == 14, "ankan only after draw");

      var suitId = tileType.SuitId;
      var index = tileType.Index;
      ConcealedTiles[tileType.TileTypeId] -= 4;
      Base5Hashes[suitId] -= 4 * Base5.Table[tileType.Index];
      _meldCount += 1;
      _melds[suitId] <<= 6;
      _melds[suitId] += 1 + 7 + 9 + index;

      if (suitId < 3)
      {
        SuitClassifiers[suitId].SetMelds(_melds[suitId]);
        UpdateValue(suitId);
      }
      else
      {
        ArrangementValues[3] = HonorClassifier.Ankan();
      }

      _scoringData.Ankan(tileType, Base5Hashes[suitId]);
    }

    public void Daiminkan(TileType tileType)
    {
      Debug.Assert(TilesInHand() == 13, "daiminkan only after discard");

      var suitId = tileType.SuitId;
      var index = tileType.Index;
      InHandByType[tileType.TileTypeId] += 1;
      ConcealedTiles[tileType.TileTypeId] -= 3;
      Base5Hashes[suitId] -= 3 * Base5.Table[tileType.Index];
      _meldCount += 1;
      _melds[suitId] <<= 6;
      _melds[suitId] += 1 + 7 + 9 + index;

      if (suitId < 3)
      {
        SuitClassifiers[suitId].SetMelds(_melds[suitId]);
        UpdateValue(suitId);
      }
      else
      {
        ArrangementValues[3] = HonorClassifier.Daiminkan();
      }

      _scoringData.Daiminkan(tileType, Base5Hashes[suitId]);
    }

    /// <summary>
    /// All tileTypeIds that would make the hand furiten if discarded.
    /// </summary>
    public IEnumerable<TileType> GetFuritenTileTypes()
    {
      Debug.Assert(TilesInHand() == 13 && Shanten == 0, "furiten only makes sense at tenpai");

      var ukeIre = GetUkeIreFor13();
      for (var i = 0; i < 34; i++)
      {
        if (ukeIre[i] >= 0)
        {
          yield return TileType.FromTileTypeId(i);
        }
      }
    }

    /// <summary>
    /// 34 ints, one per tileType.
    /// -1 if that tileType is not an ukeIre.
    /// 0-4 for the remaining tiles of that tileType if ukeIre.
    /// TODO it should be possible to exclude certain tile types from checking entirely, like 1z if 111Z has been called
    /// already
    /// </summary>
    public int[] GetUkeIreFor13()
    {
      Debug.Assert(TilesInHand() == 13, "It says 13 in the method name!");

      var currentShanten = CalculateShanten(ArrangementValues);

      var ukeIre = new int[34];
      var tileTypeId = 0;
      var localArrangements = new[] {ArrangementValues[0], ArrangementValues[1], ArrangementValues[2], ArrangementValues[3]};
      for (var suit = 0; suit < 3; ++suit)
      {
        for (var index = 0; index < 9; ++index)
        {
          if (InHandByType[tileTypeId] != 4)
          {
            var kyuuhaiValue = (0b100000001 >> index) & 1;
            Kokushi.Draw(kyuuhaiValue, ConcealedTiles[tileTypeId]);
            Chiitoi.Draw(ConcealedTiles[tileTypeId]);

            ConcealedTiles[tileTypeId] += 1;
            Base5Hashes[suit] += Base5.Table[index];
            localArrangements[suit] = SuitClassifiers[suit].GetValue(ConcealedTiles, suit, Base5Hashes);

            var newShanten = CalculateShanten(localArrangements);
            var a = currentShanten - newShanten;
            Debug.Assert(a >= 0 && a <= 1, "drawing a tile should always maintain ukeIre or improve it by 1");
            // this evaluates to (remaining tiles of that type) or -1 if newShanten is not better than currentShanten
            var t = (5 - InHandByType[tileTypeId]) * a - 1;
            ukeIre[tileTypeId] = t;

            ConcealedTiles[tileTypeId] -= 1;
            Base5Hashes[suit] -= Base5.Table[index];
            Kokushi.Discard(kyuuhaiValue, ConcealedTiles[tileTypeId]);
            Chiitoi.Discard(ConcealedTiles[tileTypeId]);
          }
          else
          {
            ukeIre[tileTypeId] = -1;
          }

          tileTypeId += 1;
        }

        localArrangements[suit] = ArrangementValues[suit];
      }

      for (var index = 0; index < 7; ++index)
      {
        if (InHandByType[tileTypeId] != 4)
        {
          var previousTileCount = ConcealedTiles[tileTypeId];
          Kokushi.Draw(1, previousTileCount);
          Chiitoi.Draw(previousTileCount);
          localArrangements[3] = HonorClassifier.Clone().Draw(ConcealedTiles[tileTypeId], (JihaiMeldBit >> index) & 1);

          var newShanten = CalculateShanten(localArrangements);
          var a = currentShanten - newShanten;
          Debug.Assert(a >= 0 && a <= 1, "drawing a tile should always maintain ukeIre or improve it by 1");
          // this evaluates to (remaining tiles of that type) or -1 if newShanten is not better than currentShanten
          var t = (5 - InHandByType[tileTypeId]) * a - 1;
          ukeIre[27 + index] = t;

          Chiitoi.Discard(previousTileCount);
          Kokushi.Discard(1, previousTileCount);
        }
        else
        {
          ukeIre[tileTypeId] = -1;
        }

        tileTypeId += 1;
      }

      return ukeIre;
    }

    /// <summary>
    /// Does ukeIre before the draw differ from ukeIre after ankan?
    /// </summary>
    public bool IsUkeIreChangedByAnkan(TileType lastDrawTileType, TileType kanTileType)
    {
      Discard(lastDrawTileType);

      var ukeIreBeforeDraw = GetUkeIreFor13();

      Draw(lastDrawTileType);

      var kanSuit = kanTileType.SuitId;
      var kanIndex = kanTileType.Index;

      int[] ukeIreAfterKan;

      if (kanSuit < 3)
      {
        _melds[kanSuit] <<= 6;
        _melds[kanSuit] += 1 + 7 + 9 + kanIndex;
        _meldCount += 1;
        SuitClassifiers[kanSuit].SetMelds(_melds[kanSuit]);
        ConcealedTiles[kanTileType.TileTypeId] -= 4;
        UpdateValue(kanSuit);

        ukeIreAfterKan = GetUkeIreFor13();

        _melds[kanSuit] >>= 6;
        _meldCount -= 1;
        SuitClassifiers[kanSuit].SetMelds(_melds[kanSuit]);
        ConcealedTiles[kanTileType.TileTypeId] += 4;
        UpdateValue(kanSuit);
      }
      else
      {
        var hc = HonorClassifier.Clone();
        var a = ArrangementValues[3];
        ArrangementValues[3] = HonorClassifier.Ankan();
        ConcealedTiles[kanTileType.TileTypeId] -= 4;
        _meldCount += 1;

        ukeIreAfterKan = GetUkeIreFor13();

        _meldCount -= 1;
        ConcealedTiles[kanTileType.TileTypeId] += 4;
        ArrangementValues[3] = a;
        HonorClassifier = hc;
      }

      return !ukeIreAfterKan.SequenceEqual(ukeIreBeforeDraw);
    }

    public int ShantenAfterDiscard(TileType tileType)
    {
      Discard(tileType);

      var shantenAfterDiscard = CalculateShanten(ArrangementValues) - 1;

      Draw(tileType);

      return shantenAfterDiscard;
    }

    public int ShantenWithTile(TileType tileType)
    {
      Debug.Assert(TilesInHand() == 13, "Too many tiles in hand to draw");

      Draw(tileType);

      var shantenWithTile = CalculateShanten(ArrangementValues) - 1;

      Discard(tileType);

      return shantenWithTile;
    }

    private readonly int[] _melds = new int[4]; // identified by meldId, youngest meld in least significant bits

    private protected readonly int[] ArrangementValues = new int[4];
    private protected readonly int[] Base5Hashes = new int[4]; // base 5 representation of concealed suits.
    private protected readonly byte[] ConcealedTiles = new byte[34];
    private protected readonly byte[] InHandByType = new byte[34]; // tiles in hand by tile type, including melds, kan is 4 tiles here
    private protected readonly SuitClassifier[] SuitClassifiers = {new(), new(), new()};
    private int _meldCount;
    private ProgressiveScoringData _scoringData = new();
    private protected ChiitoiClassifier Chiitoi = ChiitoiClassifier.Create();
    private protected ProgressiveHonorClassifier HonorClassifier;
    private protected int JihaiMeldBit; // bit=1 for honor pon, least significant bit represents east wind. bit=0 for both kan and no meld.
    private protected KokushiClassifier Kokushi = KokushiClassifier.Create();

    private protected IHandCalculator CloneOnto(HandCalculator hand)
    {
      Array.Copy(ArrangementValues, hand.ArrangementValues, ArrangementValues.Length);
      Array.Copy(Base5Hashes, hand.Base5Hashes, Base5Hashes.Length);
      Array.Copy(ConcealedTiles, hand.ConcealedTiles, ConcealedTiles.Length);
      Array.Copy(InHandByType, hand.InHandByType, InHandByType.Length);
      Array.Copy(_melds, hand._melds, _melds.Length);
      hand.JihaiMeldBit = JihaiMeldBit;
      for (var i = 0; i < SuitClassifiers.Length; ++i)
      {
        hand.SuitClassifiers[i] = SuitClassifiers[i].Clone();
      }

      hand.Chiitoi = Chiitoi.Clone();
      hand.HonorClassifier = HonorClassifier.Clone();
      hand.Kokushi = Kokushi.Clone();
      hand._meldCount = _meldCount;
      hand._scoringData = _scoringData.Clone();
      return hand;
    }

    protected int TilesInHand()
    {
      return ConcealedTiles.Sum(x => x) + _meldCount * 3;
    }

    private void InitializeJihaiMelds(IEnumerable<int> meldIds)
    {
      foreach (var meldId in meldIds)
      {
        _melds[3] <<= 6;
        if (meldId < 25)
        {
          _melds[3] += 1 + meldId;
        }
        else
        {
          _melds[3] += 1 + meldId - 9;
        }

        _meldCount += 1;

        if (meldId < 7 + 9)
        {
          var index = meldId - 7;
          var tileType = index + 27;
          HonorClassifier.Draw(0, 0);
          HonorClassifier.Draw(1, 0);
          ArrangementValues[3] = HonorClassifier.Pon(2);
          JihaiMeldBit += 1 << index;
          InHandByType[tileType] += 3;

          _scoringData.Pon(TileType.FromTileTypeId(tileType), Base5Hashes[3]);
        }
        else if (meldId < 25)
        {
          var index = meldId - 16;
          var tileType = index + 27;
          HonorClassifier.Draw(0, 0);
          HonorClassifier.Draw(1, 0);
          HonorClassifier.Draw(2, 0);
          ArrangementValues[3] = HonorClassifier.Daiminkan();
          InHandByType[tileType] += 4;

          _scoringData.Ankan(TileType.FromTileTypeId(tileType), Base5Hashes[3]);
        }
        else
        {
          var index = meldId - 25;
          var tileType = index + 27;
          HonorClassifier.Draw(0, 0);
          HonorClassifier.Draw(1, 0);
          HonorClassifier.Draw(2, 0);
          ArrangementValues[3] = HonorClassifier.Daiminkan();
          InHandByType[tileType] += 4;

          _scoringData.Daiminkan(TileType.FromTileTypeId(tileType), Base5Hashes[3]);
        }
      }
    }

    private void InitializeSuitMelds(IEnumerable<int> meldIds, int suitId)
    {
      foreach (var meldId in meldIds)
      {
        _melds[suitId] <<= 6;
        if (meldId < 25)
        {
          _melds[suitId] += 1 + meldId;
        }
        else
        {
          _melds[suitId] += 1 + meldId - 9;
        }

        _meldCount += 1;

        if (meldId < 7)
        {
          var start = 9 * suitId + meldId;
          InHandByType[start + 0] += 1;
          InHandByType[start + 1] += 1;
          InHandByType[start + 2] += 1;

          _scoringData.Chii(TileType.FromTileTypeId(start), Base5Hashes[suitId]);
        }
        else
        {
          var tileTypeId = 9 * suitId + (meldId - 7) % 9;
          if (meldId < 16)
          {
            InHandByType[tileTypeId] += 3;

            _scoringData.Pon(TileType.FromTileTypeId(tileTypeId), Base5Hashes[suitId]);
          }
          else if (meldId < 25)
          {
            InHandByType[tileTypeId] += 4;

            _scoringData.Ankan(TileType.FromTileTypeId(tileTypeId), Base5Hashes[suitId]);
          }
          else
          {
            InHandByType[tileTypeId] += 4;

            _scoringData.Daiminkan(TileType.FromTileTypeId(tileTypeId), Base5Hashes[suitId]);
          }
        }
      }

      SuitClassifiers[suitId].SetMelds(_melds[suitId]);
    }

    private void UpdateValue(int suit)
    {
      ArrangementValues[suit] = SuitClassifiers[suit].GetValue(ConcealedTiles, suit, Base5Hashes);
    }

    private protected int CalculateShanten(int[] arrangementValues)
    {
      var shanten = ArrangementClassifier.Classify(arrangementValues);
      if (_meldCount > 0)
      {
        return shanten;
      }

      return Math.Min(shanten, Math.Min(Kokushi.Shanten, Chiitoi.Shanten));
    }

    private string GetMeldString(int suitId, char suit)
    {
      var melds = _melds[suitId];
      var sb = new StringBuilder();
      var meldIds = new List<int>();
      for (var i = 0; i < 5; ++i)
      {
        var meldId = melds & 0b111111;
        if (meldId == 0)
        {
          break;
        }

        melds >>= 6;
        meldIds.Add(meldId - 1);
      }

      meldIds.Reverse();
      foreach (var meldId in meldIds)
      {
        sb.Append(" ");
        if (meldId < 7)
        {
          for (var m = meldId; m < meldId + 3; ++m)
          {
            sb.Append((char) ('1' + m));
          }
        }
        else if (meldId < 16)
        {
          var index = (meldId - 7) % 9;
          sb.Append((char) ('1' + index), InHandByType[suitId * 9 + index] - ConcealedTiles[suitId * 9 + index]);
        }
        else
        {
          var index = (meldId - 7) % 9;
          sb.Append((char) ('1' + index), InHandByType[suitId * 9 + index] - ConcealedTiles[suitId * 9 + index]);
        }

        sb.Append(suit);
      }

      return sb.ToString();
    }

    private string GetHonorMeldString()
    {
      var sb = new StringBuilder();
      for (var i = 0; i < 7; ++i)
      {
        var count = InHandByType[27 + i] - ConcealedTiles[27 + i];
        if (count > 0)
        {
          sb.Append((char) ('1' + i), count);
          sb.Append('Z');
        }
      }

      return sb.Length > 0 ? " " + sb : string.Empty;
    }

    private string GetConcealedString(int suitId, char suit)
    {
      var sb = new StringBuilder();
      var suitLength = suitId == 3 ? 7 : 9;
      for (var i = 0; i < suitLength; ++i)
      {
        sb.Append((char) ('1' + i), ConcealedTiles[suitId * 9 + i]);
      }

      if (sb.Length == 0)
      {
        return string.Empty;
      }

      sb.Append(suit);
      return sb.ToString();
    }
  }
}