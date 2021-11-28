using System;
using System.IO;
using System.Linq;
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
            activePlayerId = buffer[^1];
            visitor.Oya(activePlayerId);
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
            var tileId = file.ReadByte();
            var tile = Tile.FromTileId(tileId);
            visitor.Draw(activePlayerId, tile);
            break;
          }
          case 4: // Discard: 1 byte playerId, 1 byte tileId
          {
            var tileId = file.ReadByte();
            var tile = Tile.FromTileId(tileId);
            visitor.Discard(activePlayerId, tile);

            activePlayerId = (activePlayerId + 1) % 4;

            break;
          }
          case 5: // Tsumogiri: 1 byte playerId, 1 byte tileId
          {
            var tileId = file.ReadByte();
            var tile = Tile.FromTileId(tileId);
            visitor.Draw(activePlayerId, tile);
            visitor.Discard(activePlayerId, tile);

            activePlayerId = (activePlayerId + 1) % 4;

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

            if (activePlayerId != who)
            {

            }

            break;
          }
          case 7: //Pon: 1 byte who, 1 byte fromWho, 1 byte called tileId, 2 bytes tileIds from hand, 1 byte 0 (padding)
          {
            file.Read(meldBuffer);
            var who = meldBuffer[0];
            var fromWho = meldBuffer[1];

            if (activePlayerId == who && (fromWho + 1) % 4 != activePlayerId)
            {

            }

            activePlayerId = who;

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

            if (activePlayerId == who && (fromWho + 1) % 4 != activePlayerId)
            {

            }

            activePlayerId = who;

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

            if (activePlayerId != who)
            {

            }

            break;
          }
          case 10: //Ankan: 1 byte who, 1 byte who (padding), 4 bytes tileIds from hand
          {
            file.Read(meldBuffer);
            var who = meldBuffer[0];
            var tileType = TileType.FromTileId(meldBuffer[2]);
            visitor.Ankan(who, tileType);

            if (activePlayerId != who)
            {

            }

            break;
          }
          case 11: //Nuki: 1 byte who, 1 byte who (padding), 1 byte tileId, 3 bytes 0 (padding)
          {
            file.Read(meldBuffer);
            var who = meldBuffer[0];
            var tile = Tile.FromTileId(meldBuffer[2]);
            visitor.Nuki(who, tile);

            if (activePlayerId != who)
            {

            }

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
            var score = BitConverter.ToInt32(tenBuffer, 4);
            var limitKind = BitConverter.ToInt32(tenBuffer, 8); // limit kind: 1 for mangan, ... 5 for yakuman

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

            if (activePlayerId == who && (fromWho + 1) % 4 != activePlayerId)
            {

            }

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

            var payment = new PaymentInformation(fu, score, scoreChanges, yaku);

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
            var score = BitConverter.ToInt32(tenBuffer, 4);
            var limitKind = BitConverter.ToInt32(tenBuffer, 8); // limit kind: 1 for mangan, ... 5 for yakuman

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

            if (activePlayerId != who)
            {

            }

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

            var payment = new PaymentInformation(fu, score, scoreChanges, yaku);

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

            if (activePlayerId != who)
            {

            }

            visitor.DeclareRiichi(who);
            break;
          }
          case 17: //PayRiichi: 1 byte who
          {
            var who = file.ReadByte();

            if (activePlayerId != (who + 1) % 4)
            {

            }

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