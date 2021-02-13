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
  public class HandCalculator
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
      _suits = new[] {new int[9], new int[9], new int[9], _cJihai};
      _melds = new[] {new int[4], new int[4], new int[4]};
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
        _suits[suitId][index] -= 4;
        _melds[suitId][_meldCounts[suitId]] = 7 + index;
        _meldCounts[suitId] += 1;
        _meldCount += 1;
        _suitClassifiers[suitId].SetMelds(_melds[suitId], _meldCounts[suitId]);
        UpdateValue(suitId);
      }
      else
      {
        _cJihai[index] -= 4;
        _mJihai[index] += 4;
        _meldCount += 1;
      }
    }

    public void Chii(TileType lowestTileType, TileType calledTileType)
    {
      Debug.Assert(_tilesInHand == 13, "chii only after discard");
      Debug.Assert(lowestTileType.Suit != Suit.Jihai, "Not a valid suit for chii");

      var suitId = lowestTileType.SuitId;
      var lowestTileTypeIdInSuit = lowestTileType.Index;
      var calledTileTypeIdInSuit = calledTileType.Index;

      _suits[suitId][lowestTileTypeIdInSuit] -= 1;
      _suits[suitId][lowestTileTypeIdInSuit + 1] -= 1;
      _suits[suitId][lowestTileTypeIdInSuit + 2] -= 1;
      _suits[suitId][calledTileTypeIdInSuit] += 1;

      _melds[suitId][_meldCounts[suitId]] = lowestTileTypeIdInSuit;
      _meldCounts[suitId] += 1;
      _meldCount += 1;
      _tilesInHand += 1;
      _inHandByType[suitId * 9 + calledTileTypeIdInSuit] += 1;
      _suitClassifiers[suitId].SetMelds(_melds[suitId], _meldCounts[suitId]);
      UpdateValue(suitId);
    }

    public HandCalculator Clone()
    {
      var hand = new HandCalculator();
      for (var i = 0; i < _suits.Length; ++i)
      {
        Array.Copy(_suits[i], hand._suits[i], _suits[i].Length);
      }

      for (var i = 0; i < _melds.Length; ++i)
      {
        Array.Copy(_melds[i], hand._melds[i], _melds[i].Length);
      }

      Array.Copy(_inHandByType, hand._inHandByType, _inHandByType.Length);
      Array.Copy(_meldCounts, hand._meldCounts, _meldCounts.Length);
      Array.Copy(_mJihai, hand._mJihai, _mJihai.Length);
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
        _suits[suitId][index] -= 3;
        _melds[suitId][_meldCounts[suitId]] = 7 + index;
        _meldCounts[suitId] += 1;
        _meldCount += 1;
        _suitClassifiers[suitId].SetMelds(_melds[suitId], _meldCounts[suitId]);
        UpdateValue(suitId);
      }
      else
      {
        _arrangementValues[3] = _honorClassifier.Daiminkan();
        _cJihai[index] -= 3;
        _mJihai[index] += 4;
        _meldCount += 1;
      }
    }

    public void Discard(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 14, "Can't discard from hand with less than 13 tiles.");
      Debug.Assert(_inHandByType[tileType.TileTypeId] > 0, "Can't discard a tile that is not in the hand.");

      InternalDiscard(tileType);
    }

    public void Draw(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 13, "Can only draw with a 13 tile hand.");
      Debug.Assert(_inHandByType[tileType.TileTypeId] < 4, "Can't draw a tile with 4 of that tile in hand.");

      InternalDraw(tileType);
    }

    /// <summary>
    /// All tileTypeIds that would make the hand furiten if discarded.
    /// </summary>
    public IEnumerable<int> GetFuritenTileTypeIds()
    {
      Debug.Assert(_tilesInHand == 13 && Shanten == 0, "furiten only makes sense at tenpai");

      var ukeIre = GetUkeIreFor13();
      return ukeIre.Select(u => u.Key.TileTypeId);
    }

    public void Init(IEnumerable<TileType> tiles)
    {
      foreach (var tile in tiles)
      {
        var suit = (int) tile.Suit;
        var index = tile.Index;

        _inHandByType[suit * 9 + index] += 1;
        _tilesInHand += 1;
        if (suit == 3)
        {
          _arrangementValues[3] = _honorClassifier.Draw(_cJihai[index], _mJihai[index]);
          _kokushi.Draw(_cJihai[index]);
          _chiitoi.Draw(_cJihai[index]);
          _cJihai[index] += 1;
        }
        else
        {
          if (index == 0 || index == 8)
          {
            _kokushi.Draw(_suits[suit][index]);
          }

          _chiitoi.Draw(_suits[suit][index]);
          _suits[suit][index] += 1;
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
      InternalDiscard(lastDrawTileType);

      var ukeIreBeforeDraw = GetUkeIreFor13();

      InternalDraw(lastDrawTileType);

      var kanSuit = kanTileType.SuitId;
      var kanIndex = kanTileType.Index;

      Dictionary<TileType, int> ukeIreAfterKan;

      if (kanSuit < 3)
      {
        _melds[kanSuit][_meldCounts[kanSuit]] = 7 + 9 + kanIndex;
        _meldCounts[kanSuit] += 1;
        _meldCount += 1;
        _suitClassifiers[kanSuit].SetMelds(_melds[kanSuit], _meldCounts[kanSuit]);
        _suits[kanSuit][kanIndex] -= 4;
        UpdateValue(kanSuit);

        ukeIreAfterKan = GetUkeIreFor13();

        _meldCounts[kanSuit] -= 1;
        _meldCount -= 1;
        _suitClassifiers[kanSuit].SetMelds(_melds[kanSuit], _meldCounts[kanSuit]);
        _suits[kanSuit][kanIndex] += 4;
        UpdateValue(kanSuit);
      }
      else
      {
        var hc = _honorClassifier.Clone();
        var a = _arrangementValues[3];
        _arrangementValues[3] = _honorClassifier.Ankan();
        _cJihai[kanIndex] -= 4;
        _mJihai[kanIndex] += 4;
        _meldCount += 1;

        ukeIreAfterKan = GetUkeIreFor13();

        _meldCount -= 1;
        _mJihai[kanIndex] -= 4;
        _cJihai[kanIndex] += 4;
        _arrangementValues[3] = a;
        _honorClassifier = hc;
      }

      if (ukeIreBeforeDraw.Count(t => t.Value > 0) != ukeIreAfterKan.Count(t => t.Value > 0))
      {
        return true;
      }

      foreach (var tile in ukeIreBeforeDraw)
      {
        if (!ukeIreAfterKan.TryGetValue(tile.Key, out var count) || tile.Value != count)
        {
          return true;
        }
      }

      return false;
    }

    public void Pon(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 13, "pon only after discard");

      var suitId = tileType.SuitId;
      var index = tileType.Index;
      _inHandByType[tileType.TileTypeId] += 1;
      if (suitId < 3)
      {
        _suits[suitId][index] -= 2;
        _melds[suitId][_meldCounts[suitId]] = 7 + index;
        _meldCounts[suitId] += 1;
        _meldCount += 1;
        _tilesInHand += 1;
        _suitClassifiers[suitId].SetMelds(_melds[suitId], _meldCounts[suitId]);
        UpdateValue(suitId);
      }
      else
      {
        _arrangementValues[3] = _honorClassifier.Pon(_cJihai[index]);
        _cJihai[index] -= 2;
        _mJihai[index] += 3;
        _meldCount += 1;
        _tilesInHand += 1;
      }
    }

    public int ShantenAfterDiscard(TileType tileType)
    {
      InternalDiscard(tileType);

      var shantenAfterDiscard = CalculateShanten(_arrangementValues) - 1;

      InternalDraw(tileType);

      return shantenAfterDiscard;
    }

    public int ShantenWithTile(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 13, "Too many tiles in hand to draw");

      InternalDraw(tileType);

      var shantenWithTile = CalculateShanten(_arrangementValues) - 1;

      InternalDiscard(tileType);

      return shantenWithTile;
    }

    public void Shouminkan(TileType tileType)
    {
      Debug.Assert(_tilesInHand == 14, "shouminkan only after draw");

      var suitId = tileType.SuitId;
      var index = tileType.Index;

      _tilesInHand -= 1;
      if (suitId < 3)
      {
        _suits[suitId][index] -= 1;
        UpdateValue(suitId);
      }
      else
      {
        _arrangementValues[3] = _honorClassifier.Shouminkan();
        _cJihai[index] -= 1;
        _mJihai[index] += 1;
      }
    }

    public override string ToString()
    {
      return Shanten + ": " + GetConcealedString(0, 'm') + GetConcealedString(1, 'p') + GetConcealedString(2, 's') +
             GetConcealedString(3, 'z') +
             GetMeldString(0, 'M') + GetMeldString(1, 'P') + GetMeldString(2, 'S') + GetHonorMeldString();
    }

    private static readonly List<Suit> IdToSuit = new List<Suit> {Suit.Manzu, Suit.Pinzu, Suit.Souzu, Suit.Jihai};
    private readonly int[] _arrangementValues = new int[4];
    private readonly int[] _cJihai = new int[7]; // concealed honor tiles
    private readonly byte[] _inHandByType = new byte[34]; // tiles in hand by tile type, including melds, kan is 4 tiles here
    private readonly int[] _meldCounts = new int[3]; // used meldId slots for non-honors
    private readonly int[][] _melds; // non-honors, identified by meldId
    private readonly int[] _mJihai = new int[7]; // melded honor tiles
    private readonly SuitClassifier[] _suitClassifiers = {new SuitClassifier(), new SuitClassifier(), new SuitClassifier()};
    private readonly int[][] _suits; // all four
    private ChiitoiClassifier _chiitoi = ChiitoiClassifier.Create();
    private ProgressiveHonorClassifier _honorClassifier;
    private KokushiClassifier _kokushi = KokushiClassifier.Create();
    private int _meldCount;
    private int _tilesInHand; // concealed and melded tiles, but all melds count as 3 tiles

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
          _mJihai[index] += 3;
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
          _mJihai[index] += 4;
          _inHandByType[tileType] += 4;
        }
      }
    }

    private void InitializeSuitMelds(IEnumerable<int> meldIds, int suitId)
    {
      foreach (var meldId in meldIds)
      {
        _melds[suitId][_meldCounts[suitId]] = meldId;
        _meldCounts[suitId] += 1;
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

      _suitClassifiers[suitId].SetMelds(_melds[suitId], _meldCounts[suitId]);
    }

    private Dictionary<TileType, int> GetUkeIreFor13()
    {
      var currentShanten = CalculateShanten(_arrangementValues);

      var ukeIre = new Dictionary<TileType, int>();
      var tileType = 0;
      var localArrangements = new[] {_arrangementValues[0], _arrangementValues[1], _arrangementValues[2], _arrangementValues[3]};
      for (var suit = 0; suit < 3; ++suit)
      {
        for (var index = 0; index < 9; ++index)
        {
          if (_inHandByType[tileType] != 4)
          {
            if (index == 0 || index == 8)
            {
              _kokushi.Draw(_suits[suit][index]);
            }

            _chiitoi.Draw(_suits[suit][index]);

            _suits[suit][index] += 1;
            localArrangements[suit] = _suitClassifiers[suit].GetValue(_suits[suit]);
            if (CalculateShanten(localArrangements) < currentShanten)
            {
              ukeIre.Add(TileType.FromSuitAndIndex(IdToSuit[suit], index), 4 - _inHandByType[tileType]);
            }

            _chiitoi.Discard(_suits[suit][index]);
            if (index == 0 || index == 8)
            {
              _kokushi.Discard(_suits[suit][index]);
            }

            _suits[suit][index] -= 1;
          }

          tileType += 1;
        }

        localArrangements[suit] = _arrangementValues[suit];
      }

      for (var index = 0; index < 7; ++index)
      {
        if (_inHandByType[tileType] != 4)
        {
          var previousTileCount = _cJihai[index];
          _kokushi.Draw(previousTileCount);
          _chiitoi.Draw(previousTileCount);
          localArrangements[3] = _honorClassifier.Clone().Draw(_cJihai[index], _mJihai[index]);
          if (CalculateShanten(localArrangements) < currentShanten)
          {
            ukeIre.Add(TileType.FromSuitAndIndex(Suit.Jihai, index), 4 - _inHandByType[tileType]);
          }

          _chiitoi.Discard(previousTileCount + 1);
          _kokushi.Discard(previousTileCount + 1);
        }

        tileType += 1;
      }

      return ukeIre;
    }

    private void InternalDiscard(TileType tileType)
    {
      var suit = tileType.SuitId;
      var index = tileType.Index;

      _inHandByType[suit * 9 + index] -= 1;
      _tilesInHand -= 1;
      if (suit == 3)
      {
        _kokushi.Discard(_cJihai[index]);
        _chiitoi.Discard(_cJihai[index]);
        _cJihai[index] -= 1;
        _arrangementValues[3] = _honorClassifier.Discard(_cJihai[index], _mJihai[index]);
      }
      else
      {
        if (index == 0 || index == 8)
        {
          _kokushi.Discard(_suits[suit][index]);
        }

        _chiitoi.Discard(_suits[suit][index]);
        _suits[suit][index] -= 1;
        UpdateValue(suit);
      }
    }

    private void InternalDraw(TileType tileType)
    {
      var suit = tileType.SuitId;
      var index = tileType.Index;

      _inHandByType[suit * 9 + index] += 1;
      _tilesInHand += 1;
      if (suit == 3)
      {
        _arrangementValues[3] = _honorClassifier.Draw(_cJihai[index], _mJihai[index]);
        _kokushi.Draw(_cJihai[index]);
        _chiitoi.Draw(_cJihai[index]);
        _cJihai[index] += 1;
      }
      else
      {
        if (index == 0 || index == 8)
        {
          _kokushi.Draw(_suits[suit][index]);
        }

        _chiitoi.Draw(_suits[suit][index]);
        _suits[suit][index] += 1;
        UpdateValue(suit);
      }
    }

    private void UpdateValue(int suit)
    {
      _arrangementValues[suit] = _suitClassifiers[suit].GetValue(_suits[suit]);
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
      var sb = new StringBuilder();
      var meldCount = _meldCounts[suitId];
      for (var i = 0; i < meldCount; ++i)
      {
        sb.Append(" ");
        var meldId = _melds[suitId][i];
        if (meldId < 7)
        {
          for (var m = meldId; m < meldId + 3; ++m)
          {
            sb.Append((char) ('1' + m));
          }
        }
        else if (meldId < 16)
        {
          sb.Append((char) ('1' + meldId - 7), _inHandByType[suitId * 9 + meldId - 7] - _suits[suitId][meldId - 7]);
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
        if (_mJihai[i] > 0)
        {
          sb.Append((char) ('1' + i), _mJihai[i]);
          sb.Append('Z');
        }
      }

      return sb.ToString();
    }

    private string GetConcealedString(int suitId, char suit)
    {
      var sb = new StringBuilder();
      var tiles = _suits[suitId];
      for (var i = 0; i < tiles.Length; ++i)
      {
        sb.Append((char) ('1' + i), tiles[i]);
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