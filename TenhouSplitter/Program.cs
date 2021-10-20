using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Newtonsoft.Json;
using Spines.Mahjong.Analysis.Replay;

namespace TenhouSplitter
{
  class Program
  {
    private static string _sourceFile;
    private static string _targetDirectory;

    static void Main(string[] args)
    {
      if (args.Length != 2)
      {
        Console.WriteLine("args: <source file> <target directory>");
        Console.ReadKey();
        return;
      }

      _sourceFile = args[0];
      _targetDirectory = args[1];

      if (!File.Exists(_sourceFile))
      {
        Console.WriteLine("source file does not exist");
        Console.ReadKey();
        return;
      }

      if (!Directory.Exists(_targetDirectory))
      {
        Console.WriteLine("target directory does not exist");
        Console.ReadKey();
        return;
      }
      
      var xml = XElement.Load(_sourceFile);
      var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_sourceFile);
      var targetFileName = Path.Combine(_targetDirectory, fileNameWithoutExtension + ".split.txt");

      using (var targetFile = File.CreateText(targetFileName))
      {
        targetFile.WriteLine($"http://tenhou.net/0/?log={fileNameWithoutExtension!.Substring(0, 31)}&tw=0");
        targetFile.WriteLine();

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

    private class JsonRoot
    {
      [JsonProperty("title")]
      public string[] Title { get; } = {"", ""};
      
      [JsonProperty("name")]
      public string[] Name { get; } = { "", "", "", "" };

      [JsonProperty("rule")]
      public Rule Rule { get; } = new();

      [JsonProperty("log")]
      public object[][][] Log
      {
        get
        {
          return new []
          {
            new[]
            {
              _round.Cast<object>().ToArray(),
              _scores.Cast<object>().ToArray(),
              _doraIndicators.Cast<object>().ToArray(),
              _uraDoraIndicators.Cast<object>().ToArray(),
              _haipai[0].Cast<object>().ToArray(),
              _draws[0].Cast<object>().ToArray(),
              _discards[0].Cast<object>().ToArray(),
              _haipai[1].Cast<object>().ToArray(),
              _draws[1].Cast<object>().ToArray(),
              _discards[1].Cast<object>().ToArray(),
              _haipai[2].Cast<object>().ToArray(),
              _draws[2].Cast<object>().ToArray(),
              _discards[2].Cast<object>().ToArray(),
              _haipai[3].Cast<object>().ToArray(),
              _draws[3].Cast<object>().ToArray(),
              _discards[3].Cast<object>().ToArray(),
              _owari
            }
          };
        }
      }

      private readonly int[] _scores = {25000, 25000, 25000, 25000};
      private readonly int[] _round = {0, 0, 0};

      private readonly int[][] _haipai = { new int[13], new int[13], new int[13], new int[13] };
      private readonly List<object>[] _draws = { new(), new(), new(), new() };
      private readonly List<object>[] _discards = { new(), new(), new(), new() };
      private readonly List<int> _doraIndicators = new();
      private readonly List<int> _uraDoraIndicators = new();
      private object[] _owari = {"不明"};
      private readonly int[] _previousDraw = {-1, -1, -1, -1};

      public void SetRound(int round, int repetition, int riichiSticks)
      {
        _round[0] = round;
        _round[1] = repetition;
        _round[2] = riichiSticks;
      }

      public void AddDoraIndicator(int tileId)
      {
        _doraIndicators.Add(StrangeTileId(tileId));
      }

      public void AddUraDoraIndicator(int tileId)
      {
        _uraDoraIndicators.Add(StrangeTileId(tileId));
      }

      public void Call(int who, MeldDecoder meld)
      {
        _previousDraw[who] = -1;

        var fromWho = meld.CalledFromPlayerOffset;

        var calledTile = meld.MeldType == MeldType.ClosedKan ? meld.LowestTile : meld.CalledTile;
        var tiles = string.Join("", meld.Tiles.Except(new [] {calledTile}).Select(StrangeTileId));

        var meldIdentifier = "p";
        switch (meld.MeldType)
        {
          case MeldType.ClosedKan:
            meldIdentifier = "a";
            break;
          case MeldType.CalledKan:
            meldIdentifier = "m";
            break;
          case MeldType.AddedKan:
            meldIdentifier = "k";
            break;
          case MeldType.Koutsu:
            meldIdentifier = "p";
            break;
          case MeldType.Shuntsu:
            meldIdentifier = "c";
            break;
        }

        var insertPosition = ((4 - fromWho) % 4) * 2 - 2;
        if (meld.MeldType == MeldType.ClosedKan)
        {
          insertPosition = 6;
        }
        else if (meld.MeldType == MeldType.CalledKan && insertPosition == 4)
        {
          insertPosition += 2;
        }

        var call = tiles.Insert(insertPosition, meldIdentifier + StrangeTileId(calledTile));
        // kan from left: m11111111
        // kan across: 11m111111
        // kan from right: 111111m11
        // kan from self: 393939a39
        // shouminkan from left: k39393939
        // shouminkan from across: 39k393939
        // shouminkan from right: 3939k3939
        // chi 3 c232122
        // chi 2 c222123
        
        if (meld.MeldType == MeldType.ClosedKan || meld.MeldType == MeldType.AddedKan)
        {
          _discards[who].Add(call);
        }
        else
        {
          _draws[who].Add(call);
        }
      }

      public void Riichi(int playerIndex, int tileId)
      {
        _discards[playerIndex].Add($"r{StrangeTileId(tileId)}");
      }

      public void Draw(int playerIndex, int tileId)
      {
        _draws[playerIndex].Add(StrangeTileId(tileId));
        _previousDraw[playerIndex] = tileId;
      }

      public void Discard(int playerIndex, int tileId)
      {
        if (_previousDraw[playerIndex] == tileId)
        {
          _discards[playerIndex].Add(60);
        }
        else
        {
          _discards[playerIndex].Add(StrangeTileId(tileId));
        }
      }

      public void SetHaipai(int playerIndex, IEnumerable<int> tiles)
      {
        var i = 0;
        foreach (var tile in tiles.OrderBy(t => t))
        {
          var id = StrangeTileId(tile);
          _haipai[playerIndex][i] = id;
          i += 1;
        }
      }

      private int StrangeTileId(int tileId)
      {
        var tileType = tileId / 4;
        var suit = tileType / 9;
        var index = tileType % 9;
        var akaDora = Rule.Aka == 1 && index == 4 && suit < 3 && tileId % 4 == 0;

        var name = akaDora ? 50 + suit + 1 : (suit + 1) * 10 + index + 1;
        return name;
      }

      public void SetScore(IEnumerable<int> scores)
      {
        var i = 0;
        foreach (var score in scores)
        {
          _scores[i] = score;
          i += 1;
        }
      }

      public void SetAkaAri(bool ari)
      {
        Rule.Aka = ari ? 1 : 0;
      }

      public void Agari(int[] scoreDeltas, int who, int fromWho, int pao)
      {
        _owari = new object[3];
        _owari[0] = "和了";
        _owari[1] = scoreDeltas;
        // "跳満3000-6000点"
        _owari[2] = new object[] {who, fromWho, pao, "跳満 1点" };
      }
    }

    public class Rule
    {
      [JsonProperty("aka")]
      public int Aka { get; set; } = 1;

      [JsonProperty("disp")]
      public string Disp { get; } = "牌譜";
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
