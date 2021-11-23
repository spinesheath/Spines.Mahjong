using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Spines.Mahjong.Analysis.Replay;

namespace TenhouSplitter
{
  internal class JsonRoot
  {
    [JsonProperty("title")]
    public string[] Title { get; } = {"", ""};

    [JsonProperty("name")]
    public string[] Name { get; } = {"", "", "", ""};

    [JsonProperty("rule")]
    public Rule Rule { get; } = new();

    [JsonProperty("log")]
    public object[][][] Log
    {
      get
      {
        return new[]
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

    public void AddDoraIndicator(int tileId)
    {
      _doraIndicators.Add(StrangeTileId(tileId));
    }

    public void AddUraDoraIndicator(int tileId)
    {
      _uraDoraIndicators.Add(StrangeTileId(tileId));
    }

    public void Agari(int[] scoreDeltas, int who, int fromWho, int pao)
    {
      _owari = new object[3];
      _owari[0] = "和了";
      _owari[1] = scoreDeltas;
      // "跳満3000-6000点"
      _owari[2] = new object[] {who, fromWho, pao, "跳満 1点"};
    }

    /// <remarks>
    /// chi 3 c232122
    /// chi 2 c222123
    /// pon from left: p111111
    /// pon from across: 11p1111
    /// pon from right: 1111p11
    /// daiminkan from left: m11111111
    /// daiminkan from across: 11m111111
    /// daiminkan from right: 111111m11
    /// ankan: 393939a39
    /// shouminkan from left: k39393939
    /// shouminkan from across: 39k393939
    /// shouminkan from right: 3939k3939
    /// ankan and shouminkan go into draws, the rest into discards
    /// </remarks>
    public void Call(int who, MeldDecoder meld)
    {
      _previousDraw[who] = -1;

      var fromWho = meld.CalledFromPlayerOffset;

      var calledTile = meld.MeldType == MeldType.ClosedKan ? meld.LowestTile : meld.CalledTile;
      var tiles = string.Join("", meld.Tiles.Except(new[] {calledTile}).Select(StrangeTileId));

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

      var insertPosition = (4 - fromWho) % 4 * 2 - 2;
      if (meld.MeldType == MeldType.ClosedKan)
      {
        insertPosition = 6;
      }
      else if (meld.MeldType == MeldType.CalledKan && insertPosition == 4)
      {
        insertPosition += 2;
      }

      var call = tiles.Insert(insertPosition, meldIdentifier + StrangeTileId(calledTile));

      if (meld.MeldType == MeldType.ClosedKan || meld.MeldType == MeldType.AddedKan)
      {
        _discards[who].Add(call);
      }
      else
      {
        _draws[who].Add(call);
      }
    }

    /// <remarks>
    /// 60 means tsumogiri
    /// </remarks>
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

    public void Draw(int playerIndex, int tileId)
    {
      _draws[playerIndex].Add(StrangeTileId(tileId));
      _previousDraw[playerIndex] = tileId;
    }

    public void Riichi(int playerIndex, int tileId)
    {
      _discards[playerIndex].Add($"r{StrangeTileId(tileId)}");
    }

    public void SetAkaAri(bool ari)
    {
      Rule.Aka = ari ? 1 : 0;
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

    public void SetRound(int round, int repetition, int riichiSticks)
    {
      _round[0] = round;
      _round[1] = repetition;
      _round[2] = riichiSticks;
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

    private readonly List<object>[] _discards = {new(), new(), new(), new()};
    private readonly List<int> _doraIndicators = new();
    private readonly List<object>[] _draws = {new(), new(), new(), new()};

    private readonly int[][] _haipai = {new int[13], new int[13], new int[13], new int[13]};
    private readonly int[] _previousDraw = {-1, -1, -1, -1};
    private readonly int[] _round = {0, 0, 0};

    private readonly int[] _scores = {25000, 25000, 25000, 25000};
    private readonly List<int> _uraDoraIndicators = new();
    private object[] _owari = {"不明"};

    private int StrangeTileId(int tileId)
    {
      var tileType = tileId / 4;
      var suit = tileType / 9;
      var index = tileType % 9;
      var akaDora = Rule.Aka == 1 && index == 4 && suit < 3 && tileId % 4 == 0;

      var name = akaDora ? 50 + suit + 1 : (suit + 1) * 10 + index + 1;
      return name;
    }
  }
}