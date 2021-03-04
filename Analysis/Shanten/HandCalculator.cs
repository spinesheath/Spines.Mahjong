using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Spines.Mahjong.Analysis.Shanten
{
  /// <summary>
  /// Maintains tile counts and calculates the Shanten of a hand.
  /// </summary>
  public class HandCalculator : IHandCalculator
  {
    public HandCalculator(ShorthandParser initial)
      : this()
    {
      InitializeSuitMelds(initial.ManzuMeldIds, 0);
      InitializeSuitMelds(initial.PinzuMeldIds, 1);
      InitializeSuitMelds(initial.SouzuMeldIds, 2);
      InitializeJihaiMelds(initial.JihaiMeldIds);
      Init(initial.Tiles);
    }


    /// <summary>
    /// Creates a new instance of Hand.
    /// </summary>
    public HandCalculator()
    {
      // Don't need to initialize _arrangementValues here because the value for an empty hand is 0.
      // Don't need to set the melds in the suit classifiers here because entry state for concealed suits for a hand without melds is 0.
    }

    public override string ToString()
    {
      return Shanten + ": " + GetConcealedString(0, 'm') + GetConcealedString(1, 'p') + GetConcealedString(2, 's') +
             GetConcealedString(3, 'z') +
             GetMeldString(0, 'M') + GetMeldString(1, 'P') + GetMeldString(2, 'S') + GetHonorMeldString();
    }

    /// <summary>
    /// The current shanten of the hand.
    /// </summary>
    public int Shanten => CalculateShanten(_arrangementValues) - 1;

    public void Ankan(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 14, "ankan only after draw");

      var suitId = tileType.SuitId;
      var index = tileType.Index;

      _tilesInHand -= 1;
      if (suitId < 3)
      {
        _concealedTiles[tileType.TileTypeId] -= 4;
        _melds[suitId] <<= 6;
        _melds[suitId] += 1 + 7 + 9 + index;
        _meldCount += 1;
        _suitClassifiers[suitId].SetMelds(_melds[suitId]);
        UpdateValue(suitId);
      }
      else
      {
        _arrangementValues[3] = _honorClassifier.Ankan();
        _concealedTiles[tileType.TileTypeId] -= 4;
        _meldCount += 1;
      }
    }

    public void Chii(TileType lowestTileType, TileType calledTileType)
    {
      Debug.Assert(_tilesInHand == 13, "chii only after discard");
      Debug.Assert(lowestTileType.Suit != Suit.Jihai, "Not a valid suit for chii");

      var suitId = lowestTileType.SuitId;
      var lowestTileTypeIdInSuit = lowestTileType.Index;

      _concealedTiles[lowestTileType.TileTypeId] -= 1;
      _concealedTiles[lowestTileType.TileTypeId + 1] -= 1;
      _concealedTiles[lowestTileType.TileTypeId + 2] -= 1;
      _concealedTiles[calledTileType.TileTypeId] += 1;

      _melds[suitId] <<= 6;
      _melds[suitId] += 1 + lowestTileTypeIdInSuit;
      _meldCount += 1;
      _tilesInHand += 1;
      _inHandByType[calledTileType.TileTypeId] += 1;
      _suitClassifiers[suitId].SetMelds(_melds[suitId]);
      UpdateValue(suitId);
    }

    public IHandCalculator Clone()
    {
      var hand = new HandCalculator();
      Array.Copy(_concealedTiles, hand._concealedTiles, _concealedTiles.Length);
      Array.Copy(_melds, hand._melds, _melds.Length);
      Array.Copy(_inHandByType, hand._inHandByType, _inHandByType.Length);
      _jihaiMeldBit = hand._jihaiMeldBit;
      Array.Copy(_arrangementValues, hand._arrangementValues, _arrangementValues.Length);
      hand._tilesInHand = _tilesInHand;
      hand._meldCount = _meldCount;
      for (var i = 0; i < _suitClassifiers.Length; ++i)
      {
        hand._suitClassifiers[i] = _suitClassifiers[i].Clone();
      }

      hand._kokushi = _kokushi.Clone();
      hand._chiitoi = _chiitoi.Clone();
      hand._honorClassifier = _honorClassifier.Clone();
      return hand;
    }

    public void Daiminkan(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 13, "daiminkan only after discard");

      var suitId = tileType.SuitId;
      var index = tileType.Index;
      _inHandByType[tileType.TileTypeId] += 1;

      if (suitId < 3)
      {
        _concealedTiles[tileType.TileTypeId] -= 3;
        _melds[suitId] <<= 6;
        _melds[suitId] += 1 + 7 + 9 + index;
        _meldCount += 1;
        _suitClassifiers[suitId].SetMelds(_melds[suitId]);
        UpdateValue(suitId);
      }
      else
      {
        _arrangementValues[3] = _honorClassifier.Daiminkan();
        _concealedTiles[tileType.TileTypeId] -= 3;
        _meldCount += 1;
      }
    }

    /// <summary>
    /// All tileTypeIds that would make the hand furiten if discarded.
    /// </summary>
    public IEnumerable<TileType> GetFuritenTileTypes()
    {
      Debug.Assert(_tilesInHand == 13 && Shanten == 0, "furiten only makes sense at tenpai");

      var ukeIre = GetUkeIreFor13();
      foreach (var i in ukeIre)
      {
        if (i >= 0)
        {
          yield return TileType.FromTileTypeId(i);
        }
      }
    }

    /// <summary>
    /// 34 ints, one per tileType.
    /// -1 if that tileType is not an ukeIre.
    /// 0-4 for the remaining tiles of that tileType if ukeIre.
    /// </summary>
    public int[] GetUkeIreFor13()
    {
      Debug.Assert(_tilesInHand == 13, "It says 13 in the method name!");

      var currentShanten = CalculateShanten(_arrangementValues);

      var ukeIre = new int[34];
      var tileTypeId = 0;
      var localArrangements = new[] {_arrangementValues[0], _arrangementValues[1], _arrangementValues[2], _arrangementValues[3]};
      for (var suit = 0; suit < 3; ++suit)
      {
        for (var index = 0; index < 9; ++index)
        {
          if (_inHandByType[tileTypeId] != 4)
          {
            _kokushi.Draw(tileTypeId, _concealedTiles[tileTypeId]);
            _chiitoi.Draw(_concealedTiles[tileTypeId]);

            _concealedTiles[tileTypeId] += 1;
            _base5Hashes[suit] += Base5Table[index];
            localArrangements[suit] = _suitClassifiers[suit].GetValue(_concealedTiles, suit, _base5Hashes);

            var newShanten = CalculateShanten(localArrangements);
            var a = currentShanten - newShanten;
            Debug.Assert(a >= 0 && a <= 1, "drawing a tile should always maintain ukeIre or improve it by 1");
            // this evaluates to (remaining tiles of that type) or -1 if newShanten is not better than currentShanten
            var t = (5 - _inHandByType[tileTypeId]) * a - 1;
            ukeIre[tileTypeId] = t;

            _concealedTiles[tileTypeId] -= 1;
            _base5Hashes[suit] -= Base5Table[index];
            _kokushi.Discard(tileTypeId, _concealedTiles[tileTypeId]);
            _chiitoi.Discard(_concealedTiles[tileTypeId]);
          }
          else
          {
            ukeIre[tileTypeId] = -1;
          }

          tileTypeId += 1;
        }

        localArrangements[suit] = _arrangementValues[suit];
      }

      for (var index = 0; index < 7; ++index)
      {
        if (_inHandByType[tileTypeId] != 4)
        {
          var previousTileCount = _concealedTiles[tileTypeId];
          _kokushi.Draw(tileTypeId, previousTileCount);
          _chiitoi.Draw(previousTileCount);
          localArrangements[3] = _honorClassifier.Clone().Draw(_concealedTiles[tileTypeId], _jihaiMeldBit >> index & 1);

          var newShanten = CalculateShanten(localArrangements);
          var a = currentShanten - newShanten;
          Debug.Assert(a >= 0 && a <= 1, "drawing a tile should always maintain ukeIre or improve it by 1");
          // this evaluates to (remaining tiles of that type) or -1 if newShanten is not better than currentShanten
          var t = (5 - _inHandByType[tileTypeId]) * a - 1;
          ukeIre[27 + index] = t;

          _chiitoi.Discard(previousTileCount);
          _kokushi.Discard(tileTypeId, previousTileCount);
        }
        else
        {
          ukeIre[tileTypeId] = -1;
        }

        tileTypeId += 1;
      }

      return ukeIre;
    }

    public void Init(IEnumerable<TileType> tiles)
    {
      foreach (var tileType in tiles)
      {
        _inHandByType[tileType.TileTypeId] += 1;
        _tilesInHand += 1;

        var previousTileCount = _concealedTiles[tileType.TileTypeId]++;
        _kokushi.Draw(tileType.TileTypeId, previousTileCount);
        _chiitoi.Draw(previousTileCount);

        if (tileType.SuitId == 3)
        {
          _arrangementValues[3] = _honorClassifier.Draw(previousTileCount, _jihaiMeldBit >> tileType.Index & 1);
        }
        else
        {
          _base5Hashes[tileType.SuitId] += Base5Table[tileType.Index];
        }
      }

      UpdateValue(0);
      UpdateValue(1);
      UpdateValue(2);
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
        // TODO why is it here 7+9+kan, but just 7+kan for daiminkan?
        _melds[kanSuit] <<= 6;
        _melds[kanSuit] += 1 + 7 + 9 + kanIndex;
        _meldCount += 1;
        _suitClassifiers[kanSuit].SetMelds(_melds[kanSuit]);
        _concealedTiles[kanTileType.TileTypeId] -= 4;
        UpdateValue(kanSuit);

        ukeIreAfterKan = GetUkeIreFor13();

        _melds[kanSuit] >>= 6;
        _meldCount -= 1;
        _suitClassifiers[kanSuit].SetMelds(_melds[kanSuit]);
        _concealedTiles[kanTileType.TileTypeId] += 4;
        UpdateValue(kanSuit);
      }
      else
      {
        var hc = _honorClassifier.Clone();
        var a = _arrangementValues[3];
        _arrangementValues[3] = _honorClassifier.Ankan();
        _concealedTiles[kanTileType.TileTypeId] -= 4;
        _meldCount += 1;

        ukeIreAfterKan = GetUkeIreFor13();

        _meldCount -= 1;
        _concealedTiles[kanTileType.TileTypeId] += 4;
        _arrangementValues[3] = a;
        _honorClassifier = hc;
      }

      return !ukeIreAfterKan.SequenceEqual(ukeIreBeforeDraw);
    }

    public void Pon(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 13, "pon only after discard");

      var suitId = tileType.SuitId;
      var index = tileType.Index;
      _inHandByType[tileType.TileTypeId] += 1;
      if (suitId < 3)
      {
        _concealedTiles[tileType.TileTypeId] -= 2;
        _melds[suitId] <<= 6;
        _melds[suitId] += 1 + 7 + index;
        _meldCount += 1;
        _tilesInHand += 1;
        _suitClassifiers[suitId].SetMelds(_melds[suitId]);
        UpdateValue(suitId);
      }
      else
      {
        _arrangementValues[3] = _honorClassifier.Pon(_concealedTiles[tileType.TileTypeId]);
        _concealedTiles[tileType.TileTypeId] -= 2;
        _jihaiMeldBit += 1 << index;
        _meldCount += 1;
        _tilesInHand += 1;
      }
    }

    public int ShantenAfterDiscard(TileType tileType)
    {
      Discard(tileType);

      var shantenAfterDiscard = CalculateShanten(_arrangementValues) - 1;

      Draw(tileType);

      return shantenAfterDiscard;
    }

    public int ShantenWithTile(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 13, "Too many tiles in hand to draw");

      Draw(tileType);

      var shantenWithTile = CalculateShanten(_arrangementValues) - 1;

      Discard(tileType);

      return shantenWithTile;
    }

    public void Shouminkan(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 14, "shouminkan only after draw");

      var suitId = tileType.SuitId;

      _tilesInHand -= 1;
      if (suitId < 3)
      {
        _concealedTiles[tileType.TileTypeId] -= 1;
        UpdateValue(suitId);
      }
      else
      {
        _arrangementValues[3] = _honorClassifier.Shouminkan();
        _concealedTiles[tileType.TileTypeId] -= 1;
      }
    }

    public void Discard(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 14, "Can't discard from hand with less than 13 tiles.");
      Debug.Assert(_inHandByType[tileType.TileTypeId] > 0, "Can't discard a tile that is not in the hand.");

      _inHandByType[tileType.TileTypeId] -= 1;
      _tilesInHand -= 1;

      var tileCountAfterDiscard = --_concealedTiles[tileType.TileTypeId];
      _kokushi.Discard(tileType.TileTypeId, tileCountAfterDiscard);
      _chiitoi.Discard(tileCountAfterDiscard);

      if (tileType.SuitId == 3)
      {
        _arrangementValues[3] = _honorClassifier.Discard(tileCountAfterDiscard, _jihaiMeldBit >> tileType.Index & 1);
      }
      else
      {
        _base5Hashes[tileType.SuitId] -= Base5Table[tileType.Index];
        UpdateValue(tileType.SuitId);
      }
    }

    public void Draw(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 13, "Can only draw with a 13 tile hand.");
      Debug.Assert(_inHandByType[tileType.TileTypeId] < 4, "Can't draw a tile with 4 of that tile in hand.");

      _inHandByType[tileType.TileTypeId] += 1;
      _tilesInHand += 1;

      var previousTileCount = _concealedTiles[tileType.TileTypeId]++;
      _kokushi.Draw(tileType.TileTypeId, previousTileCount);
      _chiitoi.Draw(previousTileCount);

      if (tileType.SuitId == 3)
      {
        _arrangementValues[3] = _honorClassifier.Draw(previousTileCount, _jihaiMeldBit >> tileType.Index & 1);
      }
      else
      {
        _base5Hashes[tileType.SuitId] += Base5Table[tileType.Index];
        UpdateValue(tileType.SuitId);
      }
    }

    private readonly int[] _arrangementValues = new int[4];
    private readonly int[] _base5Hashes = new int[3]; // base 5 representation of concealed suits. Not relevant with a meld.
    private readonly byte[] _concealedTiles = new byte[34];
    private readonly byte[] _inHandByType = new byte[34]; // tiles in hand by tile type, including melds, kan is 4 tiles here
    private readonly int[] _melds = new int[3]; // non-honors, identified by meldId, youngest meld in least significant bits
    private int _jihaiMeldBit; // bit=1 for honor pon, least significant bit represents east wind. bit=0 for both kan and no meld.
    private readonly SuitClassifier[] _suitClassifiers = {new SuitClassifier(), new SuitClassifier(), new SuitClassifier()};
    private ChiitoiClassifier _chiitoi = ChiitoiClassifier.Create();
    private ProgressiveHonorClassifier _honorClassifier;
    private KokushiClassifier _kokushi = KokushiClassifier.Create();
    private int _meldCount;
    private int _tilesInHand; // concealed and melded tiles, but all melds count as 3 tiles

    private static readonly int[] Base5Table = 
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

    private void InitializeJihaiMelds(IEnumerable<int> meldIds)
    {
      foreach (var meldId in meldIds)
      {
        _meldCount += 1;
        _tilesInHand += 3;

        if (meldId < 7 + 9)
        {
          var index = meldId - 7;
          var tileType = index + 27;
          _honorClassifier.Draw(0, 0);
          _honorClassifier.Draw(1, 0);
          _arrangementValues[3] = _honorClassifier.Pon(2);
          _jihaiMeldBit += 1 << index;
          _inHandByType[tileType] += 3;
        }
        else
        {
          var index = meldId - 16;
          var tileType = index + 27;
          _honorClassifier.Draw(0, 0);
          _honorClassifier.Draw(1, 0);
          _honorClassifier.Draw(2, 0);
          _arrangementValues[3] = _honorClassifier.Daiminkan();
          _inHandByType[tileType] += 4;
        }
      }
    }

    private void InitializeSuitMelds(IEnumerable<int> meldIds, int suitId)
    {
      foreach (var meldId in meldIds)
      {
        _melds[suitId] <<= 6;
        _melds[suitId] += 1 + meldId;
        _meldCount += 1;

        if (meldId < 7)
        {
          var start = 9 * suitId + meldId;
          _inHandByType[start + 0] += 1;
          _inHandByType[start + 1] += 1;
          _inHandByType[start + 2] += 1;
        }
        else if (meldId < 16)
        {
          _inHandByType[9 * suitId + meldId - 7] += 3;
        }
        else
        {
          _inHandByType[9 * suitId + meldId - 16] += 4;
        }

        _tilesInHand += 3;
      }

      _suitClassifiers[suitId].SetMelds(_melds[suitId]);
    }

    private void UpdateValue(int suit)
    {
      _arrangementValues[suit] = _suitClassifiers[suit].GetValue(_concealedTiles, suit, _base5Hashes);
    }

    private int CalculateShanten(int[] arrangementValues)
    {
      var shanten = ArrangementClassifier.Classify(arrangementValues);
      if (_meldCount > 0)
      {
        return shanten;
      }

      return Math.Min(shanten, Math.Min(_kokushi.Shanten, _chiitoi.Shanten));
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
          sb.Append((char) ('1' + meldId - 7), _inHandByType[suitId * 9 + meldId - 7] - _concealedTiles[suitId * 9 + meldId - 7]);
        }
        else
        {
          sb.Append((char)('1' + meldId - 7 - 9), _inHandByType[suitId * 9 + meldId - 7 - 9] - _concealedTiles[suitId * 9 + meldId - 7 - 9]);
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
        var count = _inHandByType[27 + i] - _concealedTiles[27 + i];
        if (count > 0)
        {
          sb.Append((char) ('1' + i), count);
          sb.Append('Z');
        }
      }

      return sb.ToString();
    }

    private string GetConcealedString(int suitId, char suit)
    {
      var sb = new StringBuilder();
      var suitLength = suitId == 3 ? 7 : 9;
      for (var i = 0; i < suitLength; ++i)
      {
        sb.Append((char) ('1' + i), _concealedTiles[suitId * 9 + i]);
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