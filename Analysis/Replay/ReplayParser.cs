using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Spines.Mahjong.Analysis.Shanten;

namespace Spines.Mahjong.Analysis.Replay
{
  public static class ReplayParser
  {
    public static int Parse(string xml)
    {
      var shantenCalculators = new List<HandCalculator>();

      var sum = 0;

      var xElement = XElement.Parse(xml);
      foreach (var e in xElement.Elements())
      {
        var name = e.Name.LocalName;

        var drawMatch = DrawRegex.Match(name);
        if (drawMatch.Success)
        {
          var tileId = ToInt(drawMatch.Groups[2].Value);
          var playerId = drawMatch.Groups[1].Value[0] - 'T';
          shantenCalculators[playerId].Draw(tileId / 4);
          sum += shantenCalculators[playerId].Shanten < 100 ? 1 : 0;
          continue;
        }

        var discardMatch = DiscardRegex.Match(name);
        if (discardMatch.Success)
        {
          var tileId = ToInt(discardMatch.Groups[2].Value);
          var playerId = discardMatch.Groups[1].Value[0] - 'D';
          shantenCalculators[playerId].Discard(Tile.FromTileTypeId(tileId / 4));
          sum += shantenCalculators[playerId].Shanten < 100 ? 1 : 0;
          continue;
        }

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
                shantenCalculators[playerId].Init(ToInts(hai.Value).Select(t => t / 4));
                sum += shantenCalculators[playerId].Shanten < 100 ? 1 : 0;
              }
            }
            break;
          }
          case "N":
          {
            var playerId = ToInt(e.Attribute("who")?.Value);
            var decoder = new MeldDecoder(e.Attribute("m")?.Value);
            var tiles = decoder.Tiles.ToList();

            var suit = tiles[0] / 4 / 9;
            var index = tiles.Min(t => t / 4 % 9);
            switch (decoder.MeldType)
            {
              case MeldType.Shuntsu:
                shantenCalculators[playerId].Chii(suit, index, decoder.CalledTile / 4 % 9);
                break;
              case MeldType.Koutsu:
                shantenCalculators[playerId].Pon(suit, index);
                break;
              case MeldType.CalledKan:
                shantenCalculators[playerId].Daiminkan(suit, index);
                break;
              case MeldType.AddedKan:
                shantenCalculators[playerId].Shouminkan(suit, index);
                break;
              case MeldType.ClosedKan:
                shantenCalculators[playerId].Ankan(suit, index);
                break;
            }
            sum += shantenCalculators[playerId].Shanten < 100 ? 1 : 0;
            break;
          }
          default:
            throw new NotImplementedException();
        }
      }

      return sum;
    }

    private static readonly Regex DiscardRegex = new Regex(@"([DEFG])(\d{1,3})");
    private static readonly Regex DrawRegex = new Regex(@"([TUVW])(\d{1,3})");

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