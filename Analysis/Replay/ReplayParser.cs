using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.Shanten;
using Spines.Mahjong.Analysis.State;

namespace Spines.Mahjong.Analysis.Replay
{
  public static class ReplayParser
  {
    public static void Parse(FileStream file, IReplayVisitor visitor)
    {
      var meldBuffer = new byte[6];
      var haipaiBuffer = new byte[13];
      var playerCount = 4;

      while (true)
      {
        var action = file.ReadByte();
        if (action == -1)
        {
          visitor.End();
          return;
        }
        if (action == 127)
        {
          visitor.EndMatch();
          continue;
        }

        switch (action)
        {
          case 0: // GO flags: 1 byte
            var flags = (GameTypeFlag)file.ReadByte();
            if (flags.HasFlag(GameTypeFlag.Sanma))
            {
              playerCount = 3;
            }

            visitor.GameType(flags);
            break;
          case 1: // INIT seed: 6 bytes, ten: playerCount*4 bytes, oya: 1 byte
          {
            var buffer = new byte[6 + 4 * playerCount + 1];
            file.Read(buffer);
            
            var scores = new int[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
              scores[i] = BitConverter.ToInt32(buffer, i * 4 + 6) * 100;
            }

            var roundWind = TileType.FromTileTypeId(27 + buffer[0] / 4);
            visitor.Seed(roundWind, buffer[1], buffer[2], buffer[3], buffer[4], Tile.FromTileId(buffer[5]));
            visitor.Scores(scores);
            visitor.Oya(buffer[^1]);
            break;
          }
          case 2: // INIT haipai 1 byte playerId, 13 bytes tileIds
          {
            var playerId = file.ReadByte();
            file.Read(haipaiBuffer);
            var tiles = haipaiBuffer.Select(b => Tile.FromTileId(b));
            visitor.Haipai(playerId, tiles);
            break;
          }
          case 3: // Draw: 1 byte playerId, 1 byte tileId
          {
            var seatIndex = file.ReadByte();
            var tileId = file.ReadByte();
            var tile = Tile.FromTileId(tileId);
            visitor.Draw(seatIndex, tile);
            break;
          }
          case 4: // Discard: 1 byte playerId, 1 byte tileId
          {
            var seatIndex = file.ReadByte();
            var tileId = file.ReadByte();
            var tile = Tile.FromTileId(tileId);
            visitor.Discard(seatIndex, tile);
            break;
          }
          case 5: // Tsumogiri: 1 byte playerId, 1 byte tileId
          {
            var seatIndex = file.ReadByte();
            var tileId = file.ReadByte();
            var tile = Tile.FromTileId(tileId);
            visitor.Draw(seatIndex, tile);
            visitor.Discard(seatIndex, tile);
            break;
          }
          case 6: //Chii: 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand, 1 byte 0 (padding)
          {
            file.Read(meldBuffer);
            var who = meldBuffer[0];
            var fromWho = meldBuffer[1];
            var calledTile = Tile.FromTileId(meldBuffer[2]);
            var handTile0 = Tile.FromTileId(meldBuffer[3]);
            var handTile1 = Tile.FromTileId(meldBuffer[4]);
            visitor.Chii(who, fromWho, calledTile, handTile0, handTile1);
            break;
          }
          case 7: //Pon: 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand, 1 byte 0 (padding)
          {
            file.Read(meldBuffer);
            var who = meldBuffer[0];
            var fromWho = meldBuffer[1];
            var calledTile = Tile.FromTileId(meldBuffer[2]);
            var handTile0 = Tile.FromTileId(meldBuffer[3]);
            var handTile1 = Tile.FromTileId(meldBuffer[4]);
            visitor.Pon(who, fromWho, calledTile, handTile0, handTile1);
            break;
          }
          case 8: //Daiminkan: 1 byte who, 1 byte fromWho, 1 byte called tileId, 3 bytes tileIds from hand
          {
            file.Read(meldBuffer);
            var who = meldBuffer[0];
            var fromWho = meldBuffer[1];
            var calledTile = Tile.FromTileId(meldBuffer[2]);
            var handTile0 = Tile.FromTileId(meldBuffer[3]);
            var handTile1 = Tile.FromTileId(meldBuffer[4]);
            var handTile2 = Tile.FromTileId(meldBuffer[5]);
            visitor.Daiminkan(who, fromWho, calledTile, handTile0, handTile1, handTile2);
            break;
          }
          case 9: //Shouminkan: 1 byte who, 1 byte fromWho, 1 byte called tileId, 1 byte added tileId, 2 bytes tileIds from hand
          {
            file.Read(meldBuffer);
            var who = meldBuffer[0];
            var fromWho = meldBuffer[1];
            var calledTile = Tile.FromTileId(meldBuffer[2]);
            var addedTile = Tile.FromTileId(meldBuffer[3]);
            var handTile0 = Tile.FromTileId(meldBuffer[4]);
            var handTile1 = Tile.FromTileId(meldBuffer[5]);
            visitor.Shouminkan(who, fromWho, calledTile, addedTile, handTile0, handTile1);
            break;
          }
          case 10: //Ankan: 1 byte who, 1 byte who (padding), 4 bytes tileIds from hand
          {
            file.Read(meldBuffer);
            var who = meldBuffer[0];
            var tileType = TileType.FromTileId(meldBuffer[2]);
            visitor.Ankan(who, tileType);
            break;
          }
          case 11: //Nuki: 1 byte who, 1 byte who (padding), 1 byte tileId, 3 bytes 0 (padding)
          {
            file.Read(meldBuffer);
            var who = meldBuffer[0];
            var tile = Tile.FromTileId(meldBuffer[2]);
            visitor.Nuki(who, tile);
            break;
          }
          case 12: //Ron
          {
            var honbaAndRiichiSticksBuffer = new byte[2];
            file.Read(honbaAndRiichiSticksBuffer); // ba

            var haiLength = file.ReadByte();
            var haiBuffer = new byte[haiLength];
            file.Read(haiBuffer);

            var meldCount = file.ReadByte();
            var ronMeldBuffer = new byte[meldCount * 7];
            file.Read(ronMeldBuffer);

            var machi = Tile.FromTileId(file.ReadByte()); // machi

            var tenBuffer = new byte[3 * 4];
            file.Read(tenBuffer); // ten
            var fu = BitConverter.ToInt32(tenBuffer, 0);
            var han = BitConverter.ToInt32(tenBuffer, 4);
            var yakumanCount = BitConverter.ToInt32(tenBuffer, 8); // limit kind: 1 for mangan, ... 5 for yakuman

            var yakuLength = file.ReadByte();
            var yakuBuffer = new byte[yakuLength];
            file.Read(yakuBuffer);

            var yakumanLength = file.ReadByte();
            var yakumanBuffer = new byte[yakumanLength];
            file.Read(yakumanBuffer);

            var yaku = ToYakuEnum(yakuBuffer, yakumanBuffer, ronMeldBuffer);

            var doraHaiLength = file.ReadByte();
            var doraBuffer = new byte[doraHaiLength];
            file.Read(doraBuffer);
            var dora = doraBuffer.Select(i => Tile.FromTileId(i));

            var doraHaiUraLength = file.ReadByte();
            var uraDoraBuffer = new byte[doraHaiUraLength];
            file.Read(uraDoraBuffer);
            var uraDora = uraDoraBuffer.Select(i => Tile.FromTileId(i));

            var who = file.ReadByte(); // who
            var fromWho = file.ReadByte(); // fromWho
            var paoWho = file.ReadByte(); // paoWho

            var scoreBuffer = new byte[2 * 4 * playerCount];
            file.Read(scoreBuffer); // sc
            var scores = new int[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
              scores[i] = BitConverter.ToInt32(scoreBuffer, i * 8);
            }

            var scoreChanges = new int[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
              scoreChanges[i] = BitConverter.ToInt32(scoreBuffer, i * 8 + 4);
            }

            var payment = new PaymentInformation(fu, han, scoreChanges, yaku);

            visitor.Ron(who, fromWho, payment);

            break;
          }
          case 13: //Tsumo
          {
            var honbaAndRiichiSticksBuffer = new byte[2];
            file.Read(honbaAndRiichiSticksBuffer); // ba
            
            var haiLength = file.ReadByte();
            var haiBuffer = new byte[haiLength];
            file.Read(haiBuffer);
            
            var meldCount = file.ReadByte();
            var tsumoMeldBuffer = new byte[meldCount * 7];
            file.Read(tsumoMeldBuffer);

            var machi = file.ReadByte(); // machi

            var tenBuffer = new byte[3 * 4];
            file.Read(tenBuffer); // ten
            var fu = BitConverter.ToInt32(tenBuffer, 0);
            var han = BitConverter.ToInt32(tenBuffer, 4);
            var yakumanCount = BitConverter.ToInt32(tenBuffer, 8); // limit kind: 1 for mangan, ... 5 for yakuman

            var yakuLength = file.ReadByte();
            var yakuBuffer = new byte[yakuLength];
            file.Read(yakuBuffer);

            var yakumanLength = file.ReadByte();
            var yakumanBuffer = new byte[yakumanLength];
            file.Read(yakumanBuffer);

            var yaku = ToYakuEnum(yakuBuffer, yakumanBuffer, tsumoMeldBuffer);

            var doraHaiLength = file.ReadByte();
            var doraBuffer = new byte[doraHaiLength];
            file.Read(doraBuffer);
            var dora = doraBuffer.Select(i => Tile.FromTileId(i));

            var doraHaiUraLength = file.ReadByte();
            var uraDoraBuffer = new byte[doraHaiUraLength];
            file.Read(uraDoraBuffer);
            var uraDora = uraDoraBuffer.Select(i => Tile.FromTileId(i));

            var who = file.ReadByte(); // who
            var fromWho = file.ReadByte(); // fromWho
            var paoWho = file.ReadByte(); // paoWho

            var scoreBuffer = new byte[2 * 4 * playerCount];
            file.Read(scoreBuffer); // sc
            var scores = new int[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
              scores[i] = BitConverter.ToInt32(scoreBuffer, i * 8);
            }

            var scoreChanges = new int[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
              scoreChanges[i] = BitConverter.ToInt32(scoreBuffer, i * 8 + 4);
            }

            var payment = new PaymentInformation(fu, han, scoreChanges, yaku);

            visitor.Tsumo(who, payment);

            break;
          }
          case 14: //Ryuukyoku: 2 byte ba, 2*4*playerCount byte score, 1 byte ryuukyokuType, 4 byte tenpaiState
          {
            var buffer = new byte[2 + 2 * 4 * playerCount + 1 + 4];
            file.Read(buffer);

            var honba = buffer[0];
            var riichiSticks = buffer[1];

            var scores = new int[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
              scores[i] = BitConverter.ToInt32(buffer, i * 8 + 2);
            }

            var scoreChanges = new int[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
              scores[i] = BitConverter.ToInt32(buffer, i * 8 + 6);
            }

            var ryuukyokuType = (RyuukyokuType) buffer[2 + 2 * 4 * playerCount];

            var isTenpai = new bool[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
              isTenpai[i] = buffer[2 + 2 * 4 * playerCount + 1 + i] != 0;
            }

            visitor.Ryuukyoku(ryuukyokuType, honba, riichiSticks, scores, scoreChanges);
            break;
          }
          case 15: //Dora: 1 byte tileId
          {
            var tileId = file.ReadByte();
            visitor.Dora(Tile.FromTileId(tileId));
            break;
          }
          case 16: //CallRiichi: 1 byte who
          {
            var who = file.ReadByte();
            visitor.DeclareRiichi(who);
            break;
          }
          case 17: //PayRiichi: 1 byte who
          {
            var who = file.ReadByte();
            visitor.PayRiichi(who);
            break;
          }
          default:
          {
            throw new NotImplementedException("Have to handle each value to read away the data");
          }
        }
      }
    }

    private static Yaku ToYakuEnum(byte[] yakuBuffer, byte[] yakumanBuffer, byte[] meldBuffer)
    {
      var isOpen = HasOpenMeld(meldBuffer);
      var lookup = isOpen ? TenhouYakuIdToOpenYaku : TenhouYakuIdToClosedYaku;
      var result = Yaku.None;
      for (var i = 0; i < yakuBuffer.Length; i += 2)
      {
        result |= lookup[yakuBuffer[i]];
      }

      for (var i = 0; i < yakumanBuffer.Length; i++)
      {
        result |= lookup[yakumanBuffer[i]];
      }

      return result;
    }

    private static bool HasOpenMeld(byte[] meldBuffer)
    {
      for (var i = 0; i < meldBuffer.Length; i += 7)
      {
        if (meldBuffer[i] != 10)
        {
          return true;
        }
      }

      return false;
    }

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

    private static readonly Yaku[] TenhouYakuIdToOpenYaku = new[]
    {
      Yaku.MenzenTsumo, // 0
      Yaku.Riichi, // 1
      Yaku.Ippatsu, // 2
      Yaku.Chankan, // 3
      Yaku.RinshanKaihou, // 4
      Yaku.HaiteiRaoyue, // 5
      Yaku.HouteiRaoyui, // 6
      Yaku.Pinfu, // 7
      Yaku.OpenTanyao, // 8
      Yaku.Iipeikou, // 9
      Yaku.JikazeTon, // 10
      Yaku.JikazeNan, // 11
      Yaku.JikazeShaa, // 12
      Yaku.JikazePei, // 13
      Yaku.BakazeTon, // 14
      Yaku.BakazeNan, // 15
      Yaku.BakazeShaa, // 16
      Yaku.BakazePei, // 17
      Yaku.Haku, // 18
      Yaku.Hatsu, // 19
      Yaku.Chun, // 20
      Yaku.DoubleRiichi, // 21
      Yaku.Chiitoitsu, // 22
      Yaku.OpenChanta, // 23
      Yaku.OpenIttsuu, // 24
      Yaku.OpenSanshokuDoujun, // 25
      Yaku.SanshokuDoukou, // 26
      Yaku.Sankantsu, // 27
      Yaku.Toitoihou, // 28
      Yaku.Sanankou, // 29
      Yaku.Shousangen, // 30
      Yaku.Honroutou, // 31
      Yaku.Ryanpeikou, // 32
      Yaku.OpenJunchan, // 33
      Yaku.OpenHonitsu, // 34
      Yaku.OpenChinitsu, // 35
      Yaku.Renhou, // 36
      Yaku.Tenhou, // 37,
      Yaku.Chiihou, // 38,
      Yaku.Daisangen, // 39,
      Yaku.Suuankou, // 40,
      Yaku.SuuankouTanki, // 41
      Yaku.Tsuuiisou, // 42
      Yaku.Ryuuiisou, // 43
      Yaku.Chinroutou, // 44
      Yaku.ChuurenPoutou, // 45
      Yaku.JunseiChuurenPoutou, // 46
      Yaku.KokushiMusou, // 47
      Yaku.KokushiMusouJuusanMen, // 48
      Yaku.Daisuushi, // 49
      Yaku.Shousuushi, // 50
      Yaku.Suukantsu, // 51
      Yaku.Dora, // 52
      Yaku.UraDora, // 53
      Yaku.AkaDora, // 54
    };

    private static readonly Yaku[] TenhouYakuIdToClosedYaku = new[]
    {
      Yaku.MenzenTsumo, // 0
      Yaku.Riichi, // 1
      Yaku.Ippatsu, // 2
      Yaku.Chankan, // 3
      Yaku.RinshanKaihou, // 4
      Yaku.HaiteiRaoyue, // 5
      Yaku.HouteiRaoyui, // 6
      Yaku.Pinfu, // 7
      Yaku.ClosedTanyao, // 8
      Yaku.Iipeikou, // 9
      Yaku.JikazeTon, // 10
      Yaku.JikazeNan, // 11
      Yaku.JikazeShaa, // 12
      Yaku.JikazePei, // 13
      Yaku.BakazeTon, // 14
      Yaku.BakazeNan, // 15
      Yaku.BakazeShaa, // 16
      Yaku.BakazePei, // 17
      Yaku.Haku, // 18
      Yaku.Hatsu, // 19
      Yaku.Chun, // 20
      Yaku.DoubleRiichi, // 21
      Yaku.Chiitoitsu, // 22
      Yaku.ClosedChanta, // 23
      Yaku.ClosedIttsuu, // 24
      Yaku.ClosedSanshokuDoujun, // 25
      Yaku.SanshokuDoukou, // 26
      Yaku.Sankantsu, // 27
      Yaku.Toitoihou, // 28
      Yaku.Sanankou, // 29
      Yaku.Shousangen, // 30
      Yaku.Honroutou, // 31
      Yaku.Ryanpeikou, // 32
      Yaku.ClosedJunchan, // 33
      Yaku.ClosedHonitsu, // 34
      Yaku.ClosedChinitsu, // 35
      Yaku.Renhou, // 36
      Yaku.Tenhou, // 37,
      Yaku.Chiihou, // 38,
      Yaku.Daisangen, // 39,
      Yaku.Suuankou, // 40,
      Yaku.SuuankouTanki, // 41
      Yaku.Tsuuiisou, // 42
      Yaku.Ryuuiisou, // 43
      Yaku.Chinroutou, // 44
      Yaku.ChuurenPoutou, // 45
      Yaku.JunseiChuurenPoutou, // 46
      Yaku.KokushiMusou, // 47
      Yaku.KokushiMusouJuusanMen, // 48
      Yaku.Daisuushi, // 49
      Yaku.Shousuushi, // 50
      Yaku.Suukantsu, // 51
      Yaku.Dora, // 52
      Yaku.UraDora, // 53
      Yaku.AkaDora, // 54
    };
  }
}