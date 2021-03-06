using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Replay
{
  public static class ReplayParser
  {
    public static int Parse(FileStream file, bool ukeIre)
    {
      Func<HandCalculator, bool, int> evaluation = (c, b) => c.Shanten < 100 ? 1 : 0;
      if (ukeIre)
      {
        evaluation = (c, b) =>
        {
          if (b)
            return c.GetUkeIreFor13().Length < 100 ? 1 : 0;
          return 0;
        };
      }

      var meldBuffer = new byte[6];
      var haipaiBuffer = new byte[13];

      var shantenCalculators = new List<HandCalculator>();
      var playerCount = 4;

      var sum = 0;

      while (true)
      {
        var action = file.ReadByte();
        if (action == -1)
        {
          return sum;
        }
        if (action == 127)
        {
          shantenCalculators = new List<HandCalculator>();
          playerCount = 4;
          continue;
        }

        switch (action)
        {
          case 0: // GO flags: 1 byte
            var flags = (GameTypeFlag)file.ReadByte();
            if (flags.HasFlag(GameTypeFlag.Sanma))
            {
              // ReSharper disable once RedundantAssignment
              // Of course it is used...
              playerCount = 3;
              return 0;
            }
            break;
          case 1: // INIT seed: 6 bytes, ten: playerCount*4 bytes, oya: 1 byte
          {
            file.Read(new byte[6 + 4 * playerCount + 1]);
            shantenCalculators = new List<HandCalculator>();
            break;
          }
          case 2: // INIT haipai 1 byte playerId, 13 bytes tileIds
          {
            var playerId = file.ReadByte();
            shantenCalculators.Add(new HandCalculator());
            file.Read(haipaiBuffer);
            shantenCalculators[playerId].Init(haipaiBuffer.Select(b => TileType.FromTileId(b)));
            sum += evaluation(shantenCalculators[playerId], true);
            break;
          }
          case 3: // Draw: 1 byte playerId, 1 byte tileId
          {
            var playerId = file.ReadByte();
            var tileType = TileType.FromTileId(file.ReadByte());
            shantenCalculators[playerId].Draw(tileType);
            sum += evaluation(shantenCalculators[playerId], false);
            break;
          }
          case 4: // Discard: 1 byte playerId, 1 byte tileId
          {
            var playerId = file.ReadByte();
            var tileType = TileType.FromTileId(file.ReadByte());
            shantenCalculators[playerId].Discard(tileType);
            sum += evaluation(shantenCalculators[playerId], true);
            break;
          }
          case 5: // Tsumogiri: 1 byte playerId, 1 byte tileId
          {
            var playerId = file.ReadByte();
            var tileType = TileType.FromTileId(file.ReadByte());
            shantenCalculators[playerId].Draw(tileType);
            sum += evaluation(shantenCalculators[playerId], false);
            shantenCalculators[playerId].Discard(tileType);
            sum += evaluation(shantenCalculators[playerId], true);
            break;
          }
          case 6: //Chii: 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand, 1 byte 0 (padding)
          {
            file.Read(meldBuffer);
            var calledTileType = TileType.FromTileId(meldBuffer[2]);
            var lowestTileType = TileType.FromTileId(meldBuffer[2..4].Min());
            shantenCalculators[meldBuffer[0]].Chii(lowestTileType, calledTileType);
            sum += evaluation(shantenCalculators[meldBuffer[0]], false);
            break;
          }
          case 7: //Pon: 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand, 1 byte 0 (padding)
          {
            file.Read(meldBuffer);
            shantenCalculators[meldBuffer[0]].Pon(TileType.FromTileId(meldBuffer[2]));
            sum += evaluation(shantenCalculators[meldBuffer[0]], false);
            break;
          }
          case 8: //Daiminkan: 1 byte who, 1 byte fromWho, 1 byte called tileId, 3 bytes tileIds from hand
          {
            file.Read(meldBuffer);
            shantenCalculators[meldBuffer[0]].Daiminkan(TileType.FromTileId(meldBuffer[2]));
            sum += evaluation(shantenCalculators[meldBuffer[0]], true);
            break;
          }
          case 9: //Shouminkan: 1 byte who, 1 byte fromWho, 1 byte called tileId, 1 byte added tileId, 2 bytes tileIds from hand
          {
            file.Read(meldBuffer);
            shantenCalculators[meldBuffer[0]].Shouminkan(TileType.FromTileId(meldBuffer[2]));
            sum += evaluation(shantenCalculators[meldBuffer[0]], true);
            break;
          }
          case 10: //Ankan: 1 byte who, 1 byte who (padding), 4 bytes tileIds from hand
          {
            file.Read(meldBuffer);
            shantenCalculators[meldBuffer[0]].Ankan(TileType.FromTileId(meldBuffer[2]));
            sum += evaluation(shantenCalculators[meldBuffer[0]], true);
            break;
          }
          case 11: //Nuki: 1 byte who, 1 byte who (padding), 1 byte tileId, 3 bytes 0 (padding)
          {
            file.Read(meldBuffer);
            break;
          }
          case 12: //Ron
          case 13: //Tsumo
          {
            file.Read(new byte[2]); // ba
            var haiLength = file.ReadByte();
            file.Read(new byte[haiLength]);
            var meldCount = file.ReadByte();
            file.Read(new byte[meldCount * 7]);
            file.ReadByte(); // machi
            file.Read(new byte[3 * 4]); // ten
            var yakuLength = file.ReadByte();
            file.Read(new byte[yakuLength]);
            var yakumanLength = file.ReadByte();
            file.Read(new byte[yakumanLength]);
            var doraHaiLength = file.ReadByte();
            file.Read(new byte[doraHaiLength]);
            var doraHaiUraLength = file.ReadByte();
            file.Read(new byte[doraHaiUraLength]);
            file.ReadByte(); // who
            file.ReadByte(); // fromWho
            file.ReadByte(); // paoWho
            file.Read(new byte[2 * 4 * playerCount]); // sc

            break;
          }
          case 14: //Ryuukyoku: 2 byte ba, 2*4*playerCount byte score, 1 byte ryuukyokuType, 4 byte tenpaiState
          {
            file.Read(new byte[2 + 2 * 4 * playerCount + 1 + 4]);
            break;
          }
          case 15: //Dora: 1 byte tileId
          {
            file.ReadByte();
            break;
          }
          case 16: //CallRiichi: 1 byte who
          {
            file.ReadByte();
            break;
          }
          case 17: //PayRiichi: 1 byte who
          {
            file.ReadByte();
            break;
          }
          default:
          {
            throw new NotImplementedException("Have to handle each value to read away the data");
          }
        }
      }
    }


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