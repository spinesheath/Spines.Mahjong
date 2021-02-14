using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Replay
{
  public static class ReplayParser
  {
    public static int Parse(XmlReader xml)
    {
      var shantenCalculators = new List<HandCalculator>();

      var sum = 0;

      while (!xml.EOF)
      {
        xml.Read();
        if (xml.NodeType != XmlNodeType.Element)
        {
          continue;
        }

        var name = xml.LocalName;
        switch (name)
        {
          case "mjloggm":
          case "SHUFFLE":
          case "TAIKYOKU":
          case "RYUUKYOKU":
          case "BYE":
          case "UN":
          case "DORA":
          case "REACH":
          case "AGARI":
            break;
          case "GO":
            xml.MoveToAttribute("type");
            var flags = (GameTypeFlag)xml.ReadContentAsInt();
            if (flags.HasFlag(GameTypeFlag.Sanma))
            {
              return 0;
            }
            break;
          case "INIT":
          {
            shantenCalculators = new List<HandCalculator>();
            for (var playerId = 0; playerId < 4; playerId++)
            {
              shantenCalculators.Add(new HandCalculator());
              if (xml.MoveToAttribute($"hai{playerId}"))
              {
                shantenCalculators[playerId].Init(ToInts(xml.Value).Select(TileType.FromTileId));
                sum += shantenCalculators[playerId].Shanten < 100 ? 1 : 0;
              }
            }

            break;
          }
          case "N":
          {
            xml.MoveToAttribute("who");
            var playerId = xml.ReadContentAsInt();
            xml.MoveToAttribute("m");
            var decoder = new MeldDecoder(xml.Value);

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

            break;
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