using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Spines.Mahjong.Analysis.Score;
using Spines.Mahjong.Analysis.State;

namespace Spines.Mahjong.Analysis.Replay
{
  public static class ReplayParser
  {
    public static void Parse(Stream file, IReplayVisitor visitor)
    {
      var meldBuffer = new byte[6];
      var haipaiBuffer = new byte[13];
      var playerCount = 4;
      var activePlayerId = 0;

      var indexInBlock = 0;
      var block = new byte[1024];
      var maxIndex = file.Read(block);

#if DEBUG
      var log = new StringBuilder();
#endif


      while (maxIndex > indexInBlock)
      {
        var action = block[indexInBlock++];
        if (action == 32)
        {
          maxIndex = file.Read(block);
          indexInBlock = 0;

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
            var buffer = new byte[6 + 4 * playerCount + 1];
            Array.Copy(block, indexInBlock, buffer, 0, buffer.Length);
            indexInBlock += buffer.Length;

            var scores = new int[playerCount];
            for (var i = 0; i < playerCount; i++)
            {
              scores[i] = BitConverter.ToInt32(buffer, i * 4 + 6) * 100;
            }

            var roundWind = TileType.FromTileTypeId(27 + buffer[0] / 4);
            visitor.Seed(roundWind, buffer[1], buffer[2], buffer[3], buffer[4], Tile.FromTileId(buffer[5]));
            visitor.Scores(scores);
            activePlayerId = buffer[^1];
            visitor.Oya(activePlayerId);
#if DEBUG
            log.AppendLine("init");
#endif
            break;
          }
          case 2: // INIT haipai 1 byte playerId, 13 bytes tileIds
          {
            var playerId = block[indexInBlock++];
            Array.Copy(block, indexInBlock, haipaiBuffer, 0, haipaiBuffer.Length);
            indexInBlock += haipaiBuffer.Length;
            var tiles = haipaiBuffer.Select(b => Tile.FromTileId(b));
            visitor.Haipai(playerId, tiles);
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
            Array.Copy(block, indexInBlock, meldBuffer, 0, meldBuffer.Length);
            indexInBlock += meldBuffer.Length;
            var who = meldBuffer[0];
            var fromWho = meldBuffer[1];
            var calledTile = Tile.FromTileId(meldBuffer[2]);
            var handTile0 = Tile.FromTileId(meldBuffer[3]);
            var handTile1 = Tile.FromTileId(meldBuffer[4]);
            visitor.Chii(who, fromWho, calledTile, handTile0, handTile1);

#if DEBUG
            Debug.Assert(activePlayerId == who);
            log.AppendLine("chii");
#endif
            break;
          }
          case 7: //Pon: 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand, 1 byte 0 (padding)
          {
            Array.Copy(block, indexInBlock, meldBuffer, 0, meldBuffer.Length);
            indexInBlock += meldBuffer.Length;
            var who = meldBuffer[0];
            var fromWho = meldBuffer[1];
            var calledTile = Tile.FromTileId(meldBuffer[2]);
            var handTile0 = Tile.FromTileId(meldBuffer[3]);
            var handTile1 = Tile.FromTileId(meldBuffer[4]);
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
            Array.Copy(block, indexInBlock, meldBuffer, 0, meldBuffer.Length);
            indexInBlock += meldBuffer.Length;
            var who = meldBuffer[0];
            var fromWho = meldBuffer[1];
            var calledTile = Tile.FromTileId(meldBuffer[2]);
            var handTile0 = Tile.FromTileId(meldBuffer[3]);
            var handTile1 = Tile.FromTileId(meldBuffer[4]);
            var handTile2 = Tile.FromTileId(meldBuffer[5]);
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
            Array.Copy(block, indexInBlock, meldBuffer, 0, meldBuffer.Length);
            indexInBlock += meldBuffer.Length;
            var who = meldBuffer[0];
            var fromWho = meldBuffer[1];
            var calledTile = Tile.FromTileId(meldBuffer[2]);
            var addedTile = Tile.FromTileId(meldBuffer[3]);
            var handTile0 = Tile.FromTileId(meldBuffer[4]);
            var handTile1 = Tile.FromTileId(meldBuffer[5]);
            visitor.Shouminkan(who, fromWho, calledTile, addedTile, handTile0, handTile1);
            
#if DEBUG
            Debug.Assert(activePlayerId == who);
            log.AppendLine("shouminkan");
#endif
            break;
          }
          case 10: //Ankan: 1 byte who, 1 byte who (padding), 4 bytes tileIds from hand
          {
            Array.Copy(block, indexInBlock, meldBuffer, 0, meldBuffer.Length);
            indexInBlock += meldBuffer.Length;
            var who = meldBuffer[0];
            var tileType = TileType.FromTileId(meldBuffer[2]);
            visitor.Ankan(who, tileType);
            
#if DEBUG
            Debug.Assert(activePlayerId == who);
            log.AppendLine("ankan");
#endif
            break;
          }
          case 11: //Nuki: 1 byte who, 1 byte who (padding), 1 byte tileId, 3 bytes 0 (padding)
          {
            Array.Copy(block, indexInBlock, meldBuffer, 0, meldBuffer.Length);
            indexInBlock += meldBuffer.Length;
            var who = meldBuffer[0];
            var tile = Tile.FromTileId(meldBuffer[2]);
            visitor.Nuki(who, tile);
            
#if DEBUG
            Debug.Assert(activePlayerId == who);
            log.AppendLine("nuki");
#endif
            break;
          }
          case 12: //Ron
          {
            var honbaAndRiichiSticksBuffer = new byte[2];
            Array.Copy(block, indexInBlock, honbaAndRiichiSticksBuffer, 0, honbaAndRiichiSticksBuffer.Length);
            indexInBlock += honbaAndRiichiSticksBuffer.Length;

            var haiLength = block[indexInBlock++];
            var haiBuffer = new byte[haiLength];
            Array.Copy(block, indexInBlock, haiBuffer, 0, haiBuffer.Length);
            indexInBlock += haiBuffer.Length;

            var meldCount = block[indexInBlock++];
            var ronMeldBuffer = new byte[meldCount * 7];
            Array.Copy(block, indexInBlock, ronMeldBuffer, 0, ronMeldBuffer.Length);
            indexInBlock += ronMeldBuffer.Length;

            var machi = Tile.FromTileId(block[indexInBlock++]); // machi

            var tenBuffer = new byte[3 * 4];
            Array.Copy(block, indexInBlock, tenBuffer, 0, tenBuffer.Length);
            indexInBlock += tenBuffer.Length;
            var fu = BitConverter.ToInt32(tenBuffer, 0);
            var score = BitConverter.ToInt32(tenBuffer, 4);
            var limitKind = BitConverter.ToInt32(tenBuffer, 8); // limit kind: 1 for mangan, ... 5 for yakuman

            var yakuLength = block[indexInBlock++];
            var yakuBuffer = new byte[yakuLength];
            Array.Copy(block, indexInBlock, yakuBuffer, 0, yakuBuffer.Length);
            indexInBlock += yakuBuffer.Length;

            var yakumanLength = block[indexInBlock++];
            var yakumanBuffer = new byte[yakumanLength];
            Array.Copy(block, indexInBlock, yakumanBuffer, 0, yakumanBuffer.Length);
            indexInBlock += yakumanBuffer.Length;

            var yaku = ToYakuEnum(yakuBuffer, yakumanBuffer, ronMeldBuffer);

            var doraHaiLength = block[indexInBlock++];
            var doraBuffer = new byte[doraHaiLength];
            Array.Copy(block, indexInBlock, doraBuffer, 0, doraBuffer.Length);
            indexInBlock += doraBuffer.Length;
            var dora = doraBuffer.Select(i => Tile.FromTileId(i));

            var doraHaiUraLength = block[indexInBlock++];
            var uraDoraBuffer = new byte[doraHaiUraLength];
            Array.Copy(block, indexInBlock, uraDoraBuffer, 0, uraDoraBuffer.Length);
            indexInBlock += uraDoraBuffer.Length;
            var uraDora = uraDoraBuffer.Select(i => Tile.FromTileId(i));

            var who = block[indexInBlock++]; // who
            var fromWho = block[indexInBlock++]; // fromWho
            var paoWho = block[indexInBlock++]; // paoWho

            var scoreBuffer = new byte[2 * 4 * playerCount];
            Array.Copy(block, indexInBlock, scoreBuffer, 0, scoreBuffer.Length);
            indexInBlock += scoreBuffer.Length;
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

            var payment = new PaymentInformation(fu, score, scoreChanges, yaku);

            visitor.Ron(who, fromWho, payment);
#if DEBUG
            Debug.Assert(activePlayerId != who || (fromWho + 1) % 4 == activePlayerId);
            log.AppendLine("ron");
#endif
            break;
          }
          case 13: //Tsumo
          {
            var honbaAndRiichiSticksBuffer = new byte[2];
            Array.Copy(block, indexInBlock, honbaAndRiichiSticksBuffer, 0, honbaAndRiichiSticksBuffer.Length);
            indexInBlock += honbaAndRiichiSticksBuffer.Length;

            var haiLength = block[indexInBlock++];
            var haiBuffer = new byte[haiLength];
            Array.Copy(block, indexInBlock, haiBuffer, 0, haiBuffer.Length);
            indexInBlock += haiBuffer.Length;

            var meldCount = block[indexInBlock++];
            var tsumoMeldBuffer = new byte[meldCount * 7];
            Array.Copy(block, indexInBlock, tsumoMeldBuffer, 0, tsumoMeldBuffer.Length);
            indexInBlock += tsumoMeldBuffer.Length;

            var machi = block[indexInBlock++]; // machi

            var tenBuffer = new byte[3 * 4];
            Array.Copy(block, indexInBlock, tenBuffer, 0, tenBuffer.Length);
            indexInBlock += tenBuffer.Length;
            var fu = BitConverter.ToInt32(tenBuffer, 0);
            var score = BitConverter.ToInt32(tenBuffer, 4);
            var limitKind = BitConverter.ToInt32(tenBuffer, 8); // limit kind: 1 for mangan, ... 5 for yakuman

            var yakuLength = block[indexInBlock++];
            var yakuBuffer = new byte[yakuLength];
            Array.Copy(block, indexInBlock, yakuBuffer, 0, yakuBuffer.Length);
            indexInBlock += yakuBuffer.Length;

            var yakumanLength = block[indexInBlock++];
            var yakumanBuffer = new byte[yakumanLength];
            Array.Copy(block, indexInBlock, yakumanBuffer, 0, yakumanBuffer.Length);
            indexInBlock += yakumanBuffer.Length;

            var yaku = ToYakuEnum(yakuBuffer, yakumanBuffer, tsumoMeldBuffer);

            var doraHaiLength = block[indexInBlock++];
            var doraBuffer = new byte[doraHaiLength];
            Array.Copy(block, indexInBlock, doraBuffer, 0, doraBuffer.Length);
            indexInBlock += doraBuffer.Length;
            var dora = doraBuffer.Select(i => Tile.FromTileId(i));

            var doraHaiUraLength = block[indexInBlock++];
            var uraDoraBuffer = new byte[doraHaiUraLength];
            Array.Copy(block, indexInBlock, uraDoraBuffer, 0, uraDoraBuffer.Length);
            indexInBlock += uraDoraBuffer.Length;
            var uraDora = uraDoraBuffer.Select(i => Tile.FromTileId(i));

            var who = block[indexInBlock++]; // who
            var fromWho = block[indexInBlock++]; // fromWho
            var paoWho = block[indexInBlock++]; // paoWho

            var scoreBuffer = new byte[2 * 4 * playerCount];
            Array.Copy(block, indexInBlock, scoreBuffer, 0, scoreBuffer.Length);
            indexInBlock += scoreBuffer.Length;
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

            var payment = new PaymentInformation(fu, score, scoreChanges, yaku);

            visitor.Tsumo(who, payment);
#if DEBUG
            Debug.Assert(activePlayerId == who);
            log.AppendLine("tsumo");
#endif
            break;
          }
          case 14: //Ryuukyoku: 2 byte ba, 2*4*playerCount byte score, 1 byte ryuukyokuType, 4 byte tenpaiState
          {
            var buffer = new byte[2 + 2 * 4 * playerCount + 1 + 4];
            Array.Copy(block, indexInBlock, buffer, 0, buffer.Length);
            indexInBlock += buffer.Length;

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