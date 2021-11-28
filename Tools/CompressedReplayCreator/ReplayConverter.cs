using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Spines.Mahjong.Analysis.Replay;

namespace CompressedReplayCreator
{
  internal class ReplayConverter
  {
    /// <summary>
    /// Compresses the replay from xml into actions.
    /// </summary>
    /// <returns>The player count</returns>
    public static int Compress(XmlReader xml, Stream actions)
    {
      string? queuedDraw = null;
      var playerCount = 4;

      while (!xml.EOF)
      {
        xml.Read();
        if (xml.NodeType != XmlNodeType.Element)
        {
          continue;
        }

        var name = xml.LocalName;

        if (queuedDraw != null && (name[1..] != queuedDraw[1..] || queuedDraw[0] - 'T' != name[0] - 'D'))
        {
          var playerId = queuedDraw[0] - 'T';
          WriteDraw(actions, playerId, ToByte(queuedDraw[1..]));
          queuedDraw = null;
        }

        switch (name)
        {
          case "mjloggm":
          case "SHUFFLE":
          case "UN":
          case "TAIKYOKU":
          case "BYE":
          {
            break;
          }
          case "GO":
          {
            string? type = null;

            while (xml.MoveToNextAttribute())
            {
              if (xml.LocalName == "type")
              {
                type = xml.Value;
              }
              else if (xml.LocalName == "lobby")
              {
              }
              else
              {
                throw new InvalidDataException("unknown attribute in GO");
              }
            }

            if (type == null)
            {
              throw new FormatException("missing type attribute in GO");
            }

            var flags = (GameTypeFlag)ToInt(type);
            playerCount = flags.HasFlag(GameTypeFlag.Sanma) ? 3 : 4;
            WriteGo(actions, flags);
            break;
          }
          case "DORA":
          {
            WriteDora(xml, actions);
            break;
          }
          case "REACH":
          {
            WriteReach(xml, actions);
            break;
          }
          case "RYUUKYOKU":
          {
            WriteRyuukyoku(xml, actions);
            break;
          }
          case "AGARI":
          {
            WriteAgari(xml, actions);
            break;
          }
          case "INIT":
          {
            WriteInit(xml, actions, playerCount);
            break;
          }
          case "N":
          {
            WriteNaki(xml, actions);
            break;
          }
          default:
          {
            var drawPlayerId = name[0] - 'T';
            if (drawPlayerId >= 0 && drawPlayerId < 4)
            {
              queuedDraw = name;
              continue;
            }

            var discardPlayerId = name[0] - 'D';
            if (discardPlayerId >= 0 && discardPlayerId < 4)
            {
              var tileTypeId = ToByte(name[1..]);
              if (queuedDraw != null && name[1..] == queuedDraw[1..] && queuedDraw[0] - 'T' == discardPlayerId)
              {
                WriteTsumogiri(actions, discardPlayerId, tileTypeId);
              }
              else
              {
                WriteDiscard(actions, discardPlayerId, tileTypeId);
              }

              queuedDraw = null;

              continue;
            }

            throw new InvalidDataException($"unknown element {xml.LocalName}");
          }
        }
      }

      return playerCount;
    }

    private static void WriteDiscard(Stream actions, int discardPlayerId, byte tileTypeId)
    {
      actions.WriteByte((byte) Node.Discard);
      actions.WriteByte((byte) discardPlayerId);
      actions.WriteByte(tileTypeId);
    }

    private static void WriteTsumogiri(Stream actions, int playerId, byte tileTypeId)
    {
      actions.WriteByte((byte) Node.Tsumogiri);
      actions.WriteByte((byte) playerId);
      actions.WriteByte(tileTypeId);
    }

    private static void WriteGo(Stream actions, GameTypeFlag flags)
    {
      actions.WriteByte((byte) Node.Go);
      actions.WriteByte((byte) flags);
    }

    private static void WriteDraw(Stream actions, int playerId, byte tileTypeId)
    {
      actions.WriteByte((byte) Node.Draw);
      actions.WriteByte((byte) playerId);
      actions.WriteByte(tileTypeId);
    }

    private static void WriteDora(XmlReader xml, Stream actions)
    {
      string? hai = null;

      while (xml.MoveToNextAttribute())
      {
        if (xml.LocalName == "hai")
        {
          hai = xml.Value;
        }
        else
        {
          throw new InvalidDataException("unknown attribute in DORA");
        }
      }

      if (hai == null)
      {
        throw new FormatException("missing attributes in DORA");
      }

      actions.WriteByte((byte)Node.Dora);
      actions.WriteByte(ToByte(hai));
    }

    private static void WriteReach(XmlReader xml, Stream actions)
    {
      string? who = null;
      string? step = null;

      while (xml.MoveToNextAttribute())
      {
        if (xml.LocalName == "who")
        {
          who = xml.Value;
        }
        else if (xml.LocalName == "step")
        {
          step = xml.Value;
        }
        else if (xml.LocalName == "ten")
        {
          // score after riichi, but does not exist always?
        }
        else
        {
          throw new InvalidDataException("unknown attribute in REACH");
        }
      }

      if (who == null || step == null)
      {
        throw new FormatException("missing attributes in REACH");
      }

      if (step == "1")
      {
        actions.WriteByte((byte)Node.CallRiichi);
        actions.WriteByte(ToByte(who));
      }
      else if (step == "2")
      {
        actions.WriteByte((byte)Node.PayRiichi);
        actions.WriteByte(ToByte(who));
      }
      else
      {
        throw new InvalidDataException("unknown step in REACH");
      }
    }

    private static void WriteNaki(XmlReader xml, Stream actions)
    {
      string? who = null;
      string? m = null;

      while (xml.MoveToNextAttribute())
      {
        if (xml.LocalName == "who")
        {
          who = xml.Value;
        }
        else if (xml.LocalName == "m")
        {
          m = xml.Value;
        }
        else
        {
          throw new InvalidDataException("unknown attribute in GO");
        }
      }

      if (who == null || m == null)
      {
        throw new FormatException("missing attributes in N");
      }

      WriteMeld(actions, who, xml.Value);
    }

    private static void WriteInit(XmlReader xml, Stream actions, int playerCount)
    {
      string? seed = null;
      string? ten = null;
      string? oya = null;
      var hai = new string[4];

      while (xml.MoveToNextAttribute())
      {
        if (xml.LocalName == "seed")
        {
          seed = xml.Value;
        }
        else if (xml.LocalName == "ten")
        {
          ten = xml.Value;
        }
        else if (xml.LocalName == "oya")
        {
          oya = xml.Value;
        }
        else if (xml.LocalName.StartsWith("hai"))
        {
          var playerId = xml.LocalName[3] - '0';
          hai[playerId] = xml.Value;
        }
        else
        {
          throw new InvalidDataException("unknown attribute in INIT");
        }
      }

      if (seed == null || ten == null || oya == null || hai.Any(h => h == null))
      {
        throw new FormatException("missing attributes in INIT");
      }

      actions.WriteByte((byte)Node.Init);
      actions.Write(ToBytes(seed));
      actions.Write(ToInts(ten).SelectMany(BitConverter.GetBytes).ToArray());
      actions.WriteByte(ToByte(oya));

      for (var i = 0; i < playerCount; i++)
      {
        actions.WriteByte((byte)Node.Haipai);
        actions.WriteByte((byte)i);
        actions.Write(ToBytes(hai[i]));
      }
    }

    private static void WriteAgari(XmlReader xml, Stream actions)
    {
      string? ba = null;
      string? hai = null;
      string? m = null;
      string? machi = null;
      string? ten = null;
      string? yaku = null;
      string? yakuman = null;
      string? doraHai = null;
      string? doraHaiUra = null;
      string? who = null;
      string? fromWho = null;
      string? paoWho = null;
      string? sc = null;

      while (xml.MoveToNextAttribute())
      {
        if (xml.LocalName == "ba")
        {
          ba = xml.Value;
        }
        else if (xml.LocalName == "hai")
        {
          hai = xml.Value;
        }
        else if (xml.LocalName == "m")
        {
          m = xml.Value;
        }
        else if (xml.LocalName == "machi")
        {
          machi = xml.Value;
        }
        else if (xml.LocalName == "ten")
        {
          ten = xml.Value;
        }
        else if (xml.LocalName == "yaku")
        {
          yaku = xml.Value;
        }
        else if (xml.LocalName == "yakuman")
        {
          yakuman = xml.Value;
        }
        else if (xml.LocalName == "doraHai")
        {
          doraHai = xml.Value;
        }
        else if (xml.LocalName == "doraHaiUra")
        {
          doraHaiUra = xml.Value;
        }
        else if (xml.LocalName == "who")
        {
          who = xml.Value;
        }
        else if (xml.LocalName == "fromWho")
        {
          fromWho = xml.Value;
        }
        else if (xml.LocalName == "paoWho")
        {
          paoWho = xml.Value;
        }
        else if (xml.LocalName == "sc")
        {
          sc = xml.Value;
        }
        else if (xml.LocalName == "owari")
        {
        }
        else
        {
          throw new InvalidDataException("unknown attribute in AGARI");
        }
      }

      if (ba == null || hai == null || machi == null || ten == null || (yaku == null && yakuman == null) || doraHai == null || who == null || fromWho == null || sc == null)
      {
        throw new FormatException("missing attributes in AGARI");
      }

      if (who == fromWho)
      {
        actions.WriteByte((byte)Node.Tsumo);
      }
      else
      {
        actions.WriteByte((byte)Node.Ron);
      }

      actions.Write(ToBytes(ba));
      var haiBytes = ToBytes(hai);
      actions.WriteByte((byte)haiBytes.Length);
      actions.Write(haiBytes);
      if (m != null)
      {
        var meldCodes = m.Split(",");
        actions.WriteByte((byte)meldCodes.Length);
        foreach (var meldCode in meldCodes)
        {
          WriteMeld(actions, who, meldCode);
        }
      }
      else
      {
        actions.WriteByte(0);
      }

      actions.WriteByte(ToByte(machi));
      actions.Write(ToInts(ten).SelectMany(BitConverter.GetBytes).ToArray());
      var yakuBytes = ToBytes(yaku ?? "");
      actions.WriteByte((byte)yakuBytes.Length);
      actions.Write(yakuBytes);
      var yakumanBytes = ToBytes(yakuman ?? "");
      actions.WriteByte((byte)yakumanBytes.Length);
      actions.Write(yakumanBytes);
      var doraHaiBytes = ToBytes(doraHai);
      actions.WriteByte((byte)doraHaiBytes.Length);
      actions.Write(doraHaiBytes);
      var doraHaiUraBytes = ToBytes(doraHaiUra ?? "");
      actions.WriteByte((byte)doraHaiUraBytes.Length);
      actions.Write(doraHaiUraBytes);
      actions.WriteByte(ToByte(who));
      actions.WriteByte(ToByte(fromWho));
      actions.WriteByte(ToByte(paoWho ?? who));
      actions.Write(ToInts(sc).SelectMany(BitConverter.GetBytes).ToArray());
    }

    private static void WriteRyuukyoku(XmlReader xml, Stream actions)
    {
      string? ba = null;
      string? sc = null;
      string? type = null;
      var hai = new string[4];

      while (xml.MoveToNextAttribute())
      {
        if (xml.LocalName == "ba")
        {
          ba = xml.Value;
        }
        else if (xml.LocalName == "sc")
        {
          sc = xml.Value;
        }
        else if (xml.LocalName == "hai0")
        {
          hai[0] = xml.Value;
        }
        else if (xml.LocalName == "hai1")
        {
          hai[1] = xml.Value;
        }
        else if (xml.LocalName == "hai2")
        {
          hai[2] = xml.Value;
        }
        else if (xml.LocalName == "hai3")
        {
          hai[3] = xml.Value;
        }
        else if (xml.LocalName == "type")
        {
          type = xml.Value;
        }
        else if (xml.LocalName == "owari")
        {
        }
        else
        {
          throw new InvalidDataException("unknown attribute in RYUUKYOKU");
        }
      }

      if (ba == null || sc == null || hai == null)
      {
        throw new FormatException("missing attributes in RYUUKYOKU");
      }

      actions.WriteByte((byte)Node.Ryuukyoku);

      actions.Write(ToBytes(ba));
      actions.Write(ToInts(sc).SelectMany(BitConverter.GetBytes).ToArray());
      if (type == null)
      {
        actions.WriteByte((byte)RyuukyokuType.Exhaustive.Id);
      }
      else
      {
        actions.WriteByte((byte)RyuukyokuType.FromName(type).Id);
      }

      for (var i = 0; i < 4; i++)
      {
        actions.WriteByte(string.IsNullOrEmpty(hai[0]) ? (byte)0 : (byte)1);
      }
    }

    private static void WriteMeld(Stream actions, string who, string meldCode)
    {
      var decoder = new MeldDecoder(meldCode);
      var playerId = ToByte(who);
      var calledFromPlayerId = (playerId + decoder.CalledFromPlayerOffset) % 4;
      if (decoder.MeldType == MeldType.Shuntsu)
      {
        actions.WriteByte((byte)Node.Chii);
        actions.WriteByte(playerId);
        actions.WriteByte((byte)calledFromPlayerId);
        actions.WriteByte((byte)decoder.CalledTile);
        var tilesFromHand = decoder.Tiles.Except(new[] { decoder.CalledTile }).ToList();
        actions.WriteByte((byte)tilesFromHand[0]);
        actions.WriteByte((byte)tilesFromHand[1]);
        actions.WriteByte(0); // padding so all melds have the same length
      }
      else if (decoder.MeldType == MeldType.Koutsu)
      {
        actions.WriteByte((byte)Node.Pon);
        actions.WriteByte(playerId);
        actions.WriteByte((byte)calledFromPlayerId);
        actions.WriteByte((byte)decoder.CalledTile);
        var tilesFromHand = decoder.Tiles.Except(new[] { decoder.CalledTile }).ToList();
        actions.WriteByte((byte)tilesFromHand[0]);
        actions.WriteByte((byte)tilesFromHand[1]);
        actions.WriteByte(0); // padding so all melds have the same length
      }
      else if (decoder.MeldType == MeldType.CalledKan)
      {
        actions.WriteByte((byte)Node.Daiminkan);
        actions.WriteByte(playerId);
        actions.WriteByte((byte)calledFromPlayerId);
        actions.WriteByte((byte)decoder.CalledTile);
        var tilesFromHand = decoder.Tiles.Except(new[] { decoder.CalledTile }).ToList();
        actions.WriteByte((byte)tilesFromHand[0]);
        actions.WriteByte((byte)tilesFromHand[1]);
        actions.WriteByte((byte)tilesFromHand[2]);
      }
      else if (decoder.MeldType == MeldType.AddedKan)
      {
        actions.WriteByte((byte)Node.Shouminkan);
        actions.WriteByte(playerId);
        actions.WriteByte((byte)calledFromPlayerId);
        actions.WriteByte((byte)decoder.CalledTile);
        actions.WriteByte((byte)decoder.AddedTile);
        var tilesFromHand = decoder.Tiles.Except(new[] { decoder.CalledTile, decoder.AddedTile }).ToList();
        actions.WriteByte((byte)tilesFromHand[0]);
        actions.WriteByte((byte)tilesFromHand[1]);
      }
      else if (decoder.MeldType == MeldType.ClosedKan)
      {
        actions.WriteByte((byte)Node.Ankan);
        actions.WriteByte(playerId);
        actions.WriteByte(playerId); // padding so all melds have the same length
        actions.WriteByte((byte)decoder.Tiles[0]);
        actions.WriteByte((byte)decoder.Tiles[1]);
        actions.WriteByte((byte)decoder.Tiles[2]);
        actions.WriteByte((byte)decoder.Tiles[3]);
      }
      else
      {
        actions.WriteByte((byte)Node.Nuki);
        actions.WriteByte(playerId);
        actions.WriteByte(playerId); // padding so all melds have the same length
        actions.WriteByte((byte)decoder.Tiles[0]);
        actions.WriteByte(0); // padding so all melds have the same length
        actions.WriteByte(0); // padding so all melds have the same length
        actions.WriteByte(0); // padding so all melds have the same length

      }
    }

    private static IEnumerable<int> ToInts(string value)
    {
      return value?.Split(',').Select(ToInt) ?? Enumerable.Empty<int>();
    }

    private static int ToInt(string value)
    {
      return Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static byte[] ToBytes(string value)
    {
      return value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(ToByte).ToArray();
    }

    private static byte ToByte(string value)
    {
      return Convert.ToByte(value, CultureInfo.InvariantCulture);
    }
  }
}
