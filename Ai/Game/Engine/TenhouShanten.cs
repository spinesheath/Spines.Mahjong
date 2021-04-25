using System.Collections.Generic;
using Spines.Mahjong.Analysis;
using Spines.Mahjong.Analysis.Shanten;

namespace Game.Engine
{
  internal static class TenhouShanten
  {
    public static bool IsTenpai(IHandAnalysis hand, IReadOnlyList<Tile> concealedTiles, int meldCount)
    {
      if (hand.Shanten <= 0)
      {
        return true;
      }

      if (meldCount == 0)
      {
        return false;
      }

      var usedTileTypeIds = new bool[34];
      var initialTiles = new Stack<TileType>();
      foreach (var tile in concealedTiles)
      {
        usedTileTypeIds[tile.TileType.TileTypeId] = true;
        initialTiles.Push(tile.TileType);
      }

      TileType? draw = null;
      if (initialTiles.Count % 3 == 2)
      {
        draw = initialTiles.Pop();
      }

      var remainingMelds = meldCount;
      var toPon = new List<TileType>();
      for (var i = 27; i < 34 && remainingMelds > 0; i++)
      {
        if (usedTileTypeIds[i])
        {
          continue;
        }

        remainingMelds -= 1;
        var tileType = TileType.FromTileTypeId(i);
        initialTiles.Push(tileType);
        initialTiles.Push(tileType);
        initialTiles.Push(tileType);
        toPon.Add(tileType);
      }

      if (remainingMelds > 0)
      {
        return false;
      }

      var tempHand = new HandCalculator();
      tempHand.Init(initialTiles);
      foreach (var tileType in toPon)
      {
        tempHand.Pon(tileType);
        tempHand.Discard(tileType);
      }

      if (draw != null)
      {
        tempHand.Draw(draw);
      }

      return tempHand.Shanten <= 0;
    }
  }
}
