using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Replay
{
  public static class ReplayParser
  {
    public static int Parse(XElement xml)
    {
      var shantenCalculators = new List<HandCalculator>();

      var sum = 0;

      foreach (var e in xml.Elements())
      {
        var name = e.Name.LocalName;
        switch (name)
        {
          case "SHUFFLE":
          case "TAIKYOKU":
          case "GO":
          case "RYUUKYOKU":
          case "BYE":
          case "UN":
          case "DORA":
          case "REACH":
          case "AGARI":
            break;
          case "INIT":
          {
            shantenCalculators = new List<HandCalculator>();
            for (var playerId = 0; playerId < 4; playerId++)
            {
              shantenCalculators.Add(new HandCalculator());
              var hai = e.Attribute($"hai{playerId}");
              if (hai != null)
              {
                shantenCalculators[playerId].Init(ToInts(hai.Value).Select(TileType.FromTileId));
                sum += shantenCalculators[playerId].Shanten < 100 ? 1 : 0;
              }
            }
            break;
          }
          case "N":
          {
            var playerId = ToInt(e.Attribute("who")?.Value);
            var decoder = new MeldDecoder(e.Attribute("m")?.Value);

            var tileType = TileType.FromTileId(decoder.LowestTile);
            var calledTileType = TileType.FromTileId(decoder.CalledTile);

            switch (decoder.MeldType)
            {
              case MeldType.Shuntsu:
                shantenCalculators[playerId].Chii(tileType, calledTileType);
                break;
              case MeldType.Koutsu:
                shantenCalculators[playerId].Pon(tileType);
                break;
              case MeldType.CalledKan:
                shantenCalculators[playerId].Daiminkan(tileType);
                break;
              case MeldType.AddedKan:
                shantenCalculators[playerId].Shouminkan(tileType);
                break;
              case MeldType.ClosedKan:
                shantenCalculators[playerId].Ankan(tileType);
                break;
            }
            sum += shantenCalculators[playerId].Shanten < 100 ? 1 : 0;
            break;
          }
          default:
          {
            var drawId = name[0] - 'T';
            if (drawId >= 0 && drawId <= 3)
            {
              shantenCalculators[drawId].Draw(TileType.FromTileId(ToInt(name[1..])));
              sum += shantenCalculators[drawId].Shanten < 100 ? 1 : 0;
              continue;
            }

            var discardId = name[0] - 'D';
            if (discardId >= 0 && discardId <= 3)
            {
              shantenCalculators[discardId].Discard(TileType.FromTileId(ToInt(name[1..])));
              sum += shantenCalculators[discardId].Shanten < 100 ? 1 : 0;
              continue;
            }

            throw new NotImplementedException(e.ToString());
          }
        }
      }

      return sum;
    }

    private static IEnumerable<int> ToInts(string value)
    {
      return value?.Split(',').Select(ToInt) ?? Enumerable.Empty<int>();
    }

    private static int ToInt(string value)
    {
      return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }
  }
}