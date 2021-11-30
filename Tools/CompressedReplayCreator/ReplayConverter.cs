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
    public static void Compress(XmlReader xml, SanmaYonmaBundleWriter writer)
    {
      string? queuedDraw = null;

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
          writer.Draw(ToByte(queuedDraw[1..]));
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
            writer.Go(flags);
            break;
          }
          case "DORA":
          {
            WriteDora(xml, writer);
            break;
          }
          case "REACH":
          {
            WriteReach(xml, writer);
            break;
          }
          case "RYUUKYOKU":
          {
            WriteRyuukyoku(xml, writer);
            break;
          }
          case "AGARI":
          {
            WriteAgari(xml, writer);
            break;
          }
          case "INIT":
          {
            WriteInit(xml, writer);
            break;
          }
          case "N":
          {
            WriteNaki(xml, writer);
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
                writer.Tsumogiri(tileTypeId);
              }
              else
              {
                writer.Discard(tileTypeId);
              }

              queuedDraw = null;

              continue;
            }

            throw new InvalidDataException($"unknown element {xml.LocalName}");
          }
        }
      }
    }

    private static void WriteDora(XmlReader xml, SanmaYonmaBundleWriter writer)
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

      writer.Dora(ToByte(hai));
    }

    private static void WriteReach(XmlReader xml, SanmaYonmaBundleWriter writer)
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
        writer.CallRiichi(ToByte(who));
      }
      else if (step == "2")
      {
        writer.PayRiichi(ToByte(who));
      }
      else
      {
        throw new InvalidDataException("unknown step in REACH");
      }
    }

    private static void WriteNaki(XmlReader xml, SanmaYonmaBundleWriter writer)
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

      writer.Meld(ToByte(who), xml.Value);
    }

    private static void WriteInit(XmlReader xml, SanmaYonmaBundleWriter writer)
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

      writer.Init(ToBytes(seed), ToInts(ten), ToByte(oya), hai.Select(ToBytes).ToArray());
    }

    private static void WriteAgari(XmlReader xml, SanmaYonmaBundleWriter writer)
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

      writer.Agari(
        ToByte(who), 
        ToByte(fromWho), 
        ToByte(paoWho ?? who),
        ToBytes(ba),
        ToBytes(hai),
        ToInts(sc),
        ToInts(ten),
        ToByte(machi),
        ToBytes(yaku ?? ""),
        ToBytes(yakuman ?? ""),
        ToBytes(doraHai),
        ToBytes(doraHaiUra ?? ""),
        m);
    }

    private static void WriteRyuukyoku(XmlReader xml, SanmaYonmaBundleWriter writer)
    {
      string? ba = null;
      string? sc = null;
      string? type = null;
      var tenpai = new bool[4];

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
          tenpai[0] = true;
        }
        else if (xml.LocalName == "hai1")
        {
          tenpai[1] = true;
        }
        else if (xml.LocalName == "hai2")
        {
          tenpai[2] = true;
        }
        else if (xml.LocalName == "hai3")
        {
          tenpai[3] = true;
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

      if (ba == null || sc == null)
      {
        throw new FormatException("missing attributes in RYUUKYOKU");
      }

      var ryuukyokuType = type == null ? RyuukyokuType.Exhaustive : RyuukyokuType.FromName(type);
      writer.Ryuukyoku(ryuukyokuType, ToBytes(ba), ToInts(sc), tenpai);
    }

    private static IEnumerable<int> ToInts(string? value)
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
