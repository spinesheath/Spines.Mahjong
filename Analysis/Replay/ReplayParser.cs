using System;
using System.IO;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.State;

#if DEBUG
using System.Diagnostics;
using System.Text;
#endif

namespace Spines.Mahjong.Analysis.Replay
{
  public static class ReplayParser
  {
    public static void Parse(string path, IReplayVisitor visitor)
    {
      var haipai = new Tile[13];
      var scores = new int[4];
      var scoreChanges = new int[4];

      var playerCount = 4;
      var activePlayerId = 0;

      var indexInBlock = 0;
      var block = File.ReadAllBytes(path);
      var blockIndex = 0;
      var maxIndex = block.Length;

#if DEBUG
      var log = new StringBuilder();
#endif

      while (maxIndex > indexInBlock)
      {
        var action = block[indexInBlock++];
        if (action == 32)
        {
          blockIndex += 1;
          indexInBlock = blockIndex * 1024;

#if DEBUG
          log.AppendLine("block");
#endif
          continue;
        }
        
        if (action == 255)
        {
          visitor.End();
#if DEBUG
          log.AppendLine("end bundle");
#endif
          return;
        }
        if (action == 127)
        {
          visitor.EndMatch();
#if DEBUG
          log.AppendLine("end match");
#endif
          continue;
        }

        switch (action)
        {
          case 0: // GO flags: 1 byte
            var flags = (GameTypeFlag) block[indexInBlock++];
            if (flags.HasFlag(GameTypeFlag.Sanma))
            {
              playerCount = 3;
            }

            visitor.GameType(flags);
#if DEBUG
            log.AppendLine("go");
#endif
            break;
          case 1: // INIT seed: 6 bytes, ten: playerCount*4 bytes, oya: 1 byte
          {
            var roundWind = TileType.FromTileTypeId(27 + block[indexInBlock++] / 4);
            var honba = block[indexInBlock++];
            var riichiSticks = block[indexInBlock++];
            var dice0 = block[indexInBlock++];
            var dice1 = block[indexInBlock++];
            var doraIndicator = Tile.FromTileId(block[indexInBlock++]);
            
            for (var i = 0; i < playerCount; i++)
            {
              scores[i] = BitConverter.ToInt32(block, indexInBlock) * 100;
              indexInBlock += 4;
            }

            activePlayerId = block[indexInBlock++];

            visitor.Seed(roundWind, honba, riichiSticks, dice0, dice1, doraIndicator);
            visitor.Scores(scores);
            visitor.Oya(activePlayerId);
#if DEBUG
            log.AppendLine("init");
#endif
            break;
          }
          case 2: // INIT haipai 1 byte playerId, 13 bytes tileIds
          {
            var playerId = block[indexInBlock++];
            for (var i = 0; i < 13; i++)
            {
              haipai[i] = Tile.FromTileId(block[indexInBlock++]);
            }
            
            visitor.Haipai(playerId, haipai);
#if DEBUG
            log.AppendLine("haipai");
#endif
            break;
          }
          case 3: // Draw: 1 byte playerId, 1 byte tileId
          {
            var tileId = block[indexInBlock++];
            var tile = Tile.FromTileId(tileId);
            visitor.Draw(activePlayerId, tile);
#if DEBUG
            log.AppendLine("draw");
#endif
            break;
          }
          case 4: // Discard: 1 byte playerId, 1 byte tileId
          {
            var tileId = block[indexInBlock++];
            var tile = Tile.FromTileId(tileId);
            visitor.Discard(activePlayerId, tile);

            activePlayerId = (activePlayerId + 1) % 4;
#if DEBUG
            log.AppendLine("discard");
#endif
            break;
          }
          case 5: // Tsumogiri: 1 byte playerId, 1 byte tileId
          {
            var tileId = block[indexInBlock++];
            var tile = Tile.FromTileId(tileId);
            visitor.Draw(activePlayerId, tile);
            visitor.Discard(activePlayerId, tile);

            activePlayerId = (activePlayerId + 1) % 4;

#if DEBUG
            log.AppendLine("tsumogiri");
#endif
            break;
          }
          case 6: //Chii: 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand, 1 byte 0 (padding)
          {
            var who = block[indexInBlock++];
            var fromWho = block[indexInBlock++];
            var calledTile = Tile.FromTileId(block[indexInBlock++]);
            var handTile0 = Tile.FromTileId(block[indexInBlock++]);
            var handTile1 = Tile.FromTileId(block[indexInBlock++]);
            indexInBlock++;

            visitor.Chii(who, fromWho, calledTile, handTile0, handTile1);

#if DEBUG
            Debug.Assert(activePlayerId == who);
            log.AppendLine("chii");
#endif
            break;
          }
          case 7: //Pon: 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand, 1 byte 0 (padding)
          {
            var who = block[indexInBlock++];
            var fromWho = block[indexInBlock++];
            var calledTile = Tile.FromTileId(block[indexInBlock++]);
            var handTile0 = Tile.FromTileId(block[indexInBlock++]);
            var handTile1 = Tile.FromTileId(block[indexInBlock++]);
            indexInBlock++;

            visitor.Pon(who, fromWho, calledTile, handTile0, handTile1);
#if DEBUG
            Debug.Assert(activePlayerId != who || (fromWho + 1) % 4 == activePlayerId);
            log.AppendLine("pon");
#endif
            activePlayerId = who;
            break;
          }
          case 8: //Daiminkan: 1 byte who, 1 byte fromWho, 1 byte called tileId, 3 bytes tileIds from hand
          {
            var who = block[indexInBlock++];
            var fromWho = block[indexInBlock++];
            var calledTile = Tile.FromTileId(block[indexInBlock++]);
            var handTile0 = Tile.FromTileId(block[indexInBlock++]);
            var handTile1 = Tile.FromTileId(block[indexInBlock++]);
            var handTile2 = Tile.FromTileId(block[indexInBlock++]);

            visitor.Daiminkan(who, fromWho, calledTile, handTile0, handTile1, handTile2);
#if DEBUG
            Debug.Assert(activePlayerId != who || (fromWho + 1) % 4 == activePlayerId);
            log.AppendLine("daiminkan");
#endif
            activePlayerId = who;
            break;
          }
          case 9: //Shouminkan: 1 byte who, 1 byte fromWho, 1 byte called tileId, 1 byte added tileId, 2 bytes tileIds from hand
          {
            var who = block[indexInBlock++];
            var fromWho = block[indexInBlock++];
            var calledTile = Tile.FromTileId(block[indexInBlock++]);
            var addedTile = Tile.FromTileId(block[indexInBlock++]);
            var handTile0 = Tile.FromTileId(block[indexInBlock++]);
            var handTile1 = Tile.FromTileId(block[indexInBlock++]);

            visitor.Shouminkan(who, fromWho, calledTile, addedTile, handTile0, handTile1);
            
#if DEBUG
            Debug.Assert(activePlayerId == who);
            log.AppendLine("shouminkan");
#endif
            break;
          }
          case 10: //Ankan: 1 byte who, 1 byte who (padding), 4 bytes tileIds from hand
          {
            var who = block[indexInBlock++];
            indexInBlock++;
            var tileType = TileType.FromTileId(block[indexInBlock++]);
            indexInBlock++;
            indexInBlock++;
            indexInBlock++;

            visitor.Ankan(who, tileType);
            
#if DEBUG
            Debug.Assert(activePlayerId == who);
            log.AppendLine("ankan");
#endif
            break;
          }
          case 11: //Nuki: 1 byte who, 1 byte who (padding), 1 byte tileId, 3 bytes 0 (padding)
          {
            var who = block[indexInBlock++];
            indexInBlock++;
            var tile = Tile.FromTileId(block[indexInBlock++]);
            indexInBlock++;
            indexInBlock++;
            indexInBlock++;
            visitor.Nuki(who, tile);
            
#if DEBUG
            Debug.Assert(activePlayerId == who);
            log.AppendLine("nuki");
#endif
            break;
          }
          case 12: //Ron
          case 13: //Tsumo
          {
            var honba = block[indexInBlock++];
            var riichiSticks = block[indexInBlock++];

            var haiLength = block[indexInBlock++];
            indexInBlock += haiLength;

            var meldCount = block[indexInBlock++];
            var isOpen = HasOpenMeld(block, indexInBlock);
            indexInBlock += meldCount * 7;

            var machi = Tile.FromTileId(block[indexInBlock++]); // machi

            var fu = BitConverter.ToInt32(block, indexInBlock);
            var score = BitConverter.ToInt32(block, indexInBlock + 4);
            var limitKind = BitConverter.ToInt32(block, indexInBlock + 8); // limit kind: 1 for mangan, ... 5 for yakuman
            indexInBlock += 12;

            var yakuLength = block[indexInBlock++];
            var yakuOffset = indexInBlock;
            indexInBlock += yakuLength;

            var yakumanLength = block[indexInBlock++];
            var yakumanOffset = indexInBlock;
            indexInBlock += yakumanLength;

            var yaku = ToYakuEnum(block, yakuOffset, yakuLength, yakumanOffset, yakumanLength, isOpen);

            var doraHaiLength = block[indexInBlock++];
            indexInBlock += doraHaiLength;

            var doraHaiUraLength = block[indexInBlock++];
            indexInBlock += doraHaiUraLength;

            var who = block[indexInBlock++];
            var fromWho = block[indexInBlock++];
            var paoWho = block[indexInBlock++];
            
            for (var i = 0; i < playerCount; i++)
            {
              scores[i] = BitConverter.ToInt32(block, indexInBlock);
              indexInBlock += 4;
              scoreChanges[i] = BitConverter.ToInt32(block, indexInBlock);
              indexInBlock += 4;
            }

            var payment = new PaymentInformation(fu, score, scoreChanges, yaku);

            if (action == 12)
            {
              visitor.Ron(who, fromWho, payment);
#if DEBUG
              Debug.Assert(activePlayerId != who || (fromWho + 1) % 4 == activePlayerId);
              log.AppendLine("ron");
#endif
            }
            else
            {
              visitor.Tsumo(who, payment);
#if DEBUG
              Debug.Assert(activePlayerId == who);
              log.AppendLine("tsumo");
#endif
            }

            break;
          }
          case 14: //Ryuukyoku: 2 byte ba, 2*4*playerCount byte score, 1 byte ryuukyokuType, 4 byte tenpaiState
          {
            var honba = block[indexInBlock++];
            var riichiSticks = block[indexInBlock++];

            for (var i = 0; i < playerCount; i++)
            {
              scores[i] = BitConverter.ToInt32(block, indexInBlock);
              indexInBlock += 4;
              scoreChanges[i] = BitConverter.ToInt32(block, indexInBlock);
              indexInBlock += 4;
            }

            var ryuukyokuType = (RyuukyokuType) block[indexInBlock++];

            indexInBlock += 4; // tenpai states
            
            visitor.Ryuukyoku(ryuukyokuType, honba, riichiSticks, scores, scoreChanges);
#if DEBUG
            log.AppendLine("ryuukyoku");
#endif
            break;
          }
          case 15: //Dora: 1 byte tileId
          {
            var tileId = block[indexInBlock++];
            visitor.Dora(Tile.FromTileId(tileId));
#if DEBUG
            log.AppendLine("dora");
#endif
            break;
          }
          case 16: //CallRiichi: 1 byte who
          {
            var who = block[indexInBlock++];
            visitor.DeclareRiichi(who);

#if DEBUG
            Debug.Assert(activePlayerId == who);
            log.AppendLine("riichi");
#endif
            break;
          }
          case 17: //PayRiichi: 1 byte who
          {
            var who = block[indexInBlock++];
            visitor.PayRiichi(who);

#if DEBUG
            Debug.Assert(activePlayerId == (who + 1) % 4);
            log.AppendLine("pay");
#endif
            break;
          }
          default:
          {
            throw new InvalidDataException("Have to handle each value to read away the data");
          }
        }
      }
    }

    private static Yaku ToYakuEnum(byte[] block, int yakuOffset, int yakuLength, int yakumanOffset, int yakumanLength, bool isOpen)
    {
      var lookup = isOpen ? TenhouYakuIdToOpenYaku : TenhouYakuIdToClosedYaku;
      var result = Yaku.None;
      for (var i = 0; i < yakuLength; i += 2)
      {
        result |= lookup[block[yakuOffset + i]];
      }

      for (var i = 0; i < yakumanLength; i++)
      {
        result |= lookup[block[yakumanOffset + i]];
      }

      return result;
    }

    private static bool HasOpenMeld(byte[] meldBuffer, int offset)
    {
      for (var i = 0; i < meldBuffer.Length; i += 7)
      {
        if (meldBuffer[offset + i] != 10)
        {
          return true;
        }
      }

      return false;
    }

    private static readonly Yaku[] TenhouYakuIdToOpenYaku = 
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
      Yaku.Daisuushii, // 49
      Yaku.Shousuushii, // 50
      Yaku.Suukantsu, // 51
      Yaku.Dora, // 52
      Yaku.UraDora, // 53
      Yaku.AkaDora, // 54
    };

    private static readonly Yaku[] TenhouYakuIdToClosedYaku = 
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
      Yaku.Daisuushii, // 49
      Yaku.Shousuushii, // 50
      Yaku.Suukantsu, // 51
      Yaku.Dora, // 52
      Yaku.UraDora, // 53
      Yaku.AkaDora, // 54
    };
  }
}