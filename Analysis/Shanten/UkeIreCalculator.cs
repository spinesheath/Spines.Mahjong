﻿using System.Diagnostics;

namespace Spines.Mahjong.Analysis.Shanten
{
  public class UkeIreCalculator : HandCalculator, IUkeIreAnalysis
  {
    public UkeIreCalculator()
    {
    }

    public UkeIreCalculator(ShorthandParser shorthandParser)
      : base(shorthandParser)
    {
    }

    /// <summary>
    /// Finds the TileTypeId of the discard with highest UkeIre while maintaining shanten.
    /// </summary>
    public int GetHighestUkeIreDiscard()
    {
      Debug.Assert(TilesInHand() == 14, "Have to be able to discard a tile");

      // If we have a winning hand, all discards will lead to worse shanten
      var currentShanten = CalculateShanten(ArrangementValues);
      if (currentShanten == 0)
      {
        return ConcealedTiles[0];
      }

      var tileTypeId = 0;
      var localArrangements = new[] { ArrangementValues[0], ArrangementValues[1], ArrangementValues[2], ArrangementValues[3] };

      var highestUkeIre = -1;
      var highestUkeIreDiscard = -1;

      for (var suit = 0; suit < 3; ++suit)
      {
        for (var index = 0; index < 9; ++index)
        {
          if (ConcealedTiles[tileTypeId] > 0)
          {
            var kyuuhaiValue = (0b100000001 >> index) & 1;
            ConcealedTiles[tileTypeId] -= 1;
            InHandByType[tileTypeId] -= 1;
            Kokushi.Discard(kyuuhaiValue, ConcealedTiles[tileTypeId]);
            Chiitoi.Discard(ConcealedTiles[tileTypeId]);
            Base5Hashes[suit] -= Base5.Table[index];
            
            localArrangements[suit] = SuitClassifiers[suit].GetValue(ConcealedTiles, suit, Base5Hashes);
            var newShanten = CalculateShanten(localArrangements);

            if (newShanten == currentShanten)
            {
              var ukeIre = SumUkeIre(currentShanten, localArrangements, HonorClassifier);
              if (ukeIre > highestUkeIre)
              {
                highestUkeIre = ukeIre;
                highestUkeIreDiscard = tileTypeId;
              }
            }

            Base5Hashes[suit] += Base5.Table[index];
            Kokushi.Draw(kyuuhaiValue, ConcealedTiles[tileTypeId]);
            Chiitoi.Draw(ConcealedTiles[tileTypeId]);
            ConcealedTiles[tileTypeId] += 1;
            InHandByType[tileTypeId] += 1;
          }

          tileTypeId += 1;
        }

        localArrangements[suit] = ArrangementValues[suit];
      }

      for (var index = 0; index < 7; ++index)
      {
        if (ConcealedTiles[tileTypeId] > 0)
        {
          ConcealedTiles[tileTypeId] -= 1;
          InHandByType[tileTypeId] -= 1;
          var tileCountAfterDiscard = ConcealedTiles[tileTypeId];
          Kokushi.Discard(1, tileCountAfterDiscard);
          Chiitoi.Discard(tileCountAfterDiscard);

          var localHonorClassifier = HonorClassifier.Clone();
          localArrangements[3] = localHonorClassifier.Discard(tileCountAfterDiscard, JihaiMeldBit >> index & 1);
          var newShanten = CalculateShanten(localArrangements);

          if (newShanten == currentShanten)
          {
            var ukeIre = SumUkeIre(currentShanten, localArrangements, localHonorClassifier);
            if (ukeIre > highestUkeIre)
            {
              highestUkeIre = ukeIre;
              highestUkeIreDiscard = tileTypeId;
            }
          }

          Chiitoi.Draw(tileCountAfterDiscard);
          Kokushi.Draw(1, tileCountAfterDiscard);
          ConcealedTiles[tileTypeId] += 1;
          InHandByType[tileTypeId] += 1;
        }

        tileTypeId += 1;
      }

      Debug.Assert(highestUkeIreDiscard != -1, "There should always be a tile to discard.");
      return highestUkeIreDiscard;
    }

    private int SumUkeIre(int currentShanten, int[] arrangements, ProgressiveHonorClassifier localHonorClassifier)
    {
      var ukeIre = 0;
      var tileTypeId = 0;
      var localArrangements = new[] { arrangements[0], arrangements[1], arrangements[2], arrangements[3] };
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
            Debug.Assert(currentShanten >= newShanten);

            var a = currentShanten - newShanten;
            var t = (4 - InHandByType[tileTypeId]) * a;
            ukeIre += t;

            ConcealedTiles[tileTypeId] -= 1;
            Base5Hashes[suit] -= Base5.Table[index];
            Kokushi.Discard(kyuuhaiValue, ConcealedTiles[tileTypeId]);
            Chiitoi.Discard(ConcealedTiles[tileTypeId]);
          }

          tileTypeId += 1;
        }

        localArrangements[suit] = arrangements[suit];
      }

      for (var index = 0; index < 7; ++index)
      {
        if (InHandByType[tileTypeId] != 4)
        {
          var previousTileCount = ConcealedTiles[tileTypeId];
          Kokushi.Draw(1, previousTileCount);
          Chiitoi.Draw(previousTileCount);

          localArrangements[3] = localHonorClassifier.Clone().Draw(previousTileCount, JihaiMeldBit >> index & 1);
          var newShanten = CalculateShanten(localArrangements);
          Debug.Assert(currentShanten >= newShanten);

          var a = currentShanten - newShanten;
          var t = (4 - InHandByType[tileTypeId]) * a;
          ukeIre += t;

          Kokushi.Discard(1, previousTileCount);
          Chiitoi.Discard(previousTileCount);
        }

        tileTypeId += 1;
      }

      return ukeIre;
    }

    private UkeIreCalculator Clone()
    {
      var c = new UkeIreCalculator();
      CloneOnto(c);
      return c;
    }

    public IUkeIreAnalysis WithChii(TileType lowestTileType, TileType calledTileType)
    {
      var c = Clone();
      c.Chii(lowestTileType, calledTileType);
      return c;
    }

    public IUkeIreAnalysis WithPon(TileType tileType)
    {
      var c = Clone();
      c.Pon(tileType);
      return c;
    }

    public IUkeIreAnalysis WithTile(TileType tileType)
    {
      var c = Clone();
      c.Draw(tileType);
      return c;
    }
  }
}
