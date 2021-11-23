using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Newtonsoft.Json;
using Spines.Mahjong.Analysis.Replay;

namespace TenhouSplitter
{
  class Program
  {
    static async Task Main(string[] args)
    {
      if (args.Length != 1)
      {
        Console.WriteLine("invalid arguments: source file or tenhou replay id");
        Console.ReadKey();
        return;
      }

      var source = args[0];
      var xml = await ReplayLoader.Load(source);

      if (xml == null)
      {
        Console.WriteLine("could not find replay");
        Console.ReadKey();
      }

      var targetFileName = Path.Combine(Path.GetTempPath(), "tenhousplitter_4CA6FAB8732343429A43026F88FCE9B5.txt");

      using (var targetFile = File.CreateText(targetFileName))
      {
        Split(xml, targetFile);
      }

      var process = new Process();
      process.StartInfo = new ProcessStartInfo
      {
        UseShellExecute = true,
        FileName = targetFileName
      };

      process.Start();
    }

    private static void Split(XElement xml, TextWriter targetFile)
    {
      var root = new JsonRoot();
      var usernames = new []{"", "", "", ""};

      var playerCount = 4;
      var aka = true;
      var repetition = 0;
      var previousRound = -1;
      var pendingRiichiDiscard = false;

      foreach (var node in xml.Elements())
      {
        var name = node.Name.LocalName;

        switch (name)
        {
          case "mjloggm":
          case "SHUFFLE":
          case "TAIKYOKU":
          case "BYE":
          {
            break;
          }
          case "UN":
          {
            for (var i = 0; i < playerCount; i++)
            {
              usernames[i] = DecodeName(node.Attribute($"n{i}")!.Value);
            }
            
            break;
          }
          case "GO":
          {
            var flags = ToInt(node.Attribute("type")!.Value);
            playerCount = (flags & 16) == 1 ? 3 : 4;
            aka = (flags & 2) != 1;
            break;
          }
          case "DORA":
          {
            root.AddDoraIndicator(ToInt(node.Attribute("hai")!.Value));
            break;
          }
          case "REACH":
          {
            if (node.Attribute("step")!.Value == "1")
            {
              pendingRiichiDiscard = true;
            }
            break;
          }

          case "RYUUKYOKU":
          {
            var roundWind = "ESWN"[previousRound / 4];
            var roundWithinWind = previousRound % 4;
            targetFile.WriteLine($"{roundWind}{roundWithinWind}-{repetition}: Draw");
            targetFile.WriteLine(JsonConvert.SerializeObject(root));
            targetFile.WriteLine();
            break;
          }
          case "AGARI":
          {
            var uraDora = node.Attribute("doraHai");
            if (uraDora != null)
            {
              var tileIds = ToInts(uraDora.Value);
              foreach (var tileId in tileIds)
              {
                root.AddUraDoraIndicator(tileId);
              }
            }

            var who = ToInt(node.Attribute("who")!.Value);
            var fromWho = ToInt(node.Attribute("fromWho")!.Value);
            var sc = ToInts(node.Attribute("sc")!.Value).ToList();
            var scoreDeltas = new int[4];
            for (var i = 0; i < 4; i++)
            {
              scoreDeltas[i] = sc[2 * i + 1] * 100;
            }

            var ten = ToInts(node.Attribute("ten")!.Value);

            var paoAttribute = node.Attribute("pao");
            var pao = paoAttribute == null ? who : ToInt(paoAttribute.Value);

            root.Agari(scoreDeltas, who, fromWho, pao);

            var roundWind = "ESWN"[previousRound / 4];
            var roundWithinWind = previousRound % 4;
            var agariName = who == fromWho ? "Tsumo" : "Ron";
            targetFile.WriteLine($"{roundWind}{roundWithinWind}-{repetition}: {usernames[who]} {agariName}");
            targetFile.WriteLine(JsonConvert.SerializeObject(root));
            targetFile.WriteLine();
            break;
          }
          case "INIT":
          {
            root = new JsonRoot();
            root.SetAkaAri(aka);
            for (int i = 0; i < playerCount; i++)
            {
              root.Name[i] = usernames[i];
            }

            var seed = node.Attribute("seed")!.Value;
            // (round indicator), (honba), (riichiSticks), (dice0), (dice1), (dora indicator)
            var stuff = ToInts(seed).ToArray();
            var round = stuff[0];
            var riichiSticks = stuff[2];
            
            if (round == previousRound)
            {
              repetition += 1;
            }

            previousRound = round;

            root.SetRound(round, repetition, riichiSticks);
            root.AddDoraIndicator(stuff[5]);
            root.SetScore(ToInts(node.Attribute("ten")!.Value).Select(s => s * 100));

            for (var i = 0; i < playerCount; i++)
            {
              root.SetHaipai(i, ToInts(node.Attribute($"hai{i}")!.Value));
            }

            break;
          }
          case "N":
          {
            var who = ToInt(node.Attribute("who")!.Value);
            var meld = new MeldDecoder(node.Attribute("m")!.Value);
            root.Call(who, meld);
            break;
          }
          default:
          {
            var drawId = name[0] - 'T';
            if (drawId >= 0 && drawId < 4)
            {
              var tileId = ToInt(name[1..]);
              root.Draw(drawId, tileId);
              continue;
            }

            var discardId = name[0] - 'D';
            if (discardId >= 0 && discardId < 4)
            {
              var tileId = ToInt(name[1..]);

              if (pendingRiichiDiscard)
              {
                pendingRiichiDiscard = false;
                root.Riichi(discardId, tileId);
              }
              else
              {
                root.Discard(discardId, tileId);
              }

              continue;
            }

            throw new NotImplementedException($"unknown element {node.Name.LocalName}");
          }
        }
      }
    }

    private static string DecodeName(string encodedName)
    {
      var encodedCharacters = encodedName.Split(new[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
      var decodedCharacters = encodedCharacters.Select(c => Convert.ToByte(c, 16)).ToArray();
      return new UTF8Encoding().GetString(decodedCharacters);
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
