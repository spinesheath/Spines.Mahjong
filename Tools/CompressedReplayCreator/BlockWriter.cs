using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;

namespace CompressedReplayCreator
{
  internal class BlockWriter
  {
    public BlockWriter(Stream stream)
    {
      _stream = stream;
    }

    public void Agari(byte who, byte fromWho, byte paoWho, byte[] ba, byte[] hai, IEnumerable<int> scores, IEnumerable<int> ten, byte machi,
      byte[] yaku, byte[] yakuman, byte[] dora, byte[] uraDora, string? meldCodes)
    {
      if (who == fromWho)
      {
        _stream.WriteByte((byte) Node.Tsumo);
      }
      else
      {
        _stream.WriteByte((byte) Node.Ron);
      }

      _stream.Write(ba);
      _stream.WriteByte((byte) hai.Length);
      _stream.Write(hai);
      if (meldCodes != null)
      {
        var splitMeldCodes = meldCodes.Split(",");
        _stream.WriteByte((byte) splitMeldCodes.Length);
        foreach (var meldCode in splitMeldCodes)
        {
          Meld(who, meldCode);
        }
      }
      else
      {
        _stream.WriteByte(0);
      }

      _stream.WriteByte(machi);
      _stream.Write(ten.SelectMany(BitConverter.GetBytes).ToArray());
      _stream.WriteByte((byte) yaku.Length);
      _stream.Write(yaku);
      _stream.WriteByte((byte) yakuman.Length);
      _stream.Write(yakuman);
      _stream.WriteByte((byte) dora.Length);
      _stream.Write(dora);
      _stream.WriteByte((byte) uraDora.Length);
      _stream.Write(uraDora);
      _stream.WriteByte(who);
      _stream.WriteByte(fromWho);
      _stream.WriteByte(paoWho);
      _stream.Write(scores.SelectMany(BitConverter.GetBytes).ToArray());
    }

    public void CallRiichi(byte who)
    {
      _stream.WriteByte((byte) Node.CallRiichi);
      _stream.WriteByte(who);
    }

    public void Discard(int discardPlayerId, byte tileId)
    {
      _stream.WriteByte((byte) Node.Discard);
      _stream.WriteByte((byte) discardPlayerId);
      _stream.WriteByte(tileId);
    }

    public void Dora(byte tileId)
    {
      _stream.WriteByte((byte) Node.Dora);
      _stream.WriteByte(tileId);
    }

    public void Draw(int playerId, byte tileId)
    {
      _stream.WriteByte((byte) Node.Draw);
      _stream.WriteByte((byte) playerId);
      _stream.WriteByte(tileId);
    }

    public void Go(GameTypeFlag flags)
    {
      _stream.WriteByte((byte) Node.Go);
      _stream.WriteByte((byte) flags);
    }

    public void Init(byte[] seed, IEnumerable<int> ten, byte oya, byte[][] hai, int playerCount)
    {
      _stream.WriteByte((byte) Node.Init);
      _stream.Write(seed);
      _stream.Write(ten.SelectMany(BitConverter.GetBytes).ToArray());
      _stream.WriteByte(oya);

      for (var i = 0; i < playerCount; i++)
      {
        _stream.WriteByte((byte) Node.Haipai);
        _stream.WriteByte((byte) i);
        _stream.Write(hai[i]);
      }
    }

    public void Meld(byte who, string meldCode)
    {
      var decoder = new MeldDecoder(meldCode);
      var playerId = who;
      var calledFromPlayerId = (playerId + decoder.CalledFromPlayerOffset) % 4;
      if (decoder.MeldType == MeldType.Shuntsu)
      {
        var tilesFromHand = decoder.Tiles.Except(new[] {decoder.CalledTile}).ToList();
        Meld(Node.Chii, playerId, calledFromPlayerId, decoder.CalledTile, tilesFromHand[0], tilesFromHand[1], 0);
      }
      else if (decoder.MeldType == MeldType.Koutsu)
      {
        var tilesFromHand = decoder.Tiles.Except(new[] {decoder.CalledTile}).ToList();
        Meld(Node.Pon, playerId, calledFromPlayerId, decoder.CalledTile, tilesFromHand[0], tilesFromHand[1], 0);
      }
      else if (decoder.MeldType == MeldType.CalledKan)
      {
        var tilesFromHand = decoder.Tiles.Except(new[] {decoder.CalledTile}).ToList();
        Meld(Node.Daiminkan, playerId, calledFromPlayerId, decoder.CalledTile, tilesFromHand[0], tilesFromHand[1], tilesFromHand[2]);
      }
      else if (decoder.MeldType == MeldType.AddedKan)
      {
        var tilesFromHand = decoder.Tiles.Except(new[] {decoder.CalledTile, decoder.AddedTile}).ToList();
        Meld(Node.Shouminkan, playerId, calledFromPlayerId, decoder.CalledTile, decoder.AddedTile, tilesFromHand[0], tilesFromHand[1]);
      }
      else if (decoder.MeldType == MeldType.ClosedKan)
      {
        Meld(Node.Ankan, playerId, playerId, decoder.Tiles[0], decoder.Tiles[1], decoder.Tiles[2], decoder.Tiles[3]);
      }
      else
      {
        Meld(Node.Nuki, playerId, playerId, decoder.Tiles[0], 0, 0, 0);
      }
    }

    public void PayRiichi(byte who)
    {
      _stream.WriteByte((byte) Node.PayRiichi);
      _stream.WriteByte(who);
    }

    public void Ryuukyoku(RyuukyokuType type, byte[] ba, IEnumerable<int> scores, bool[] tenpai)
    {
      _stream.WriteByte((byte) Node.Ryuukyoku);

      _stream.Write(ba);
      _stream.Write(scores.SelectMany(BitConverter.GetBytes).ToArray());

      _stream.WriteByte((byte) type.Id);

      for (var i = 0; i < 4; i++)
      {
        _stream.WriteByte(tenpai[i] ? (byte) 0 : (byte) 1);
      }
    }

    public void Tsumogiri(int playerId, byte tileId)
    {
      _stream.WriteByte((byte) Node.Tsumogiri);
      _stream.WriteByte((byte) playerId);
      _stream.WriteByte(tileId);
    }

    private readonly Stream _stream;

    private void Meld(Node type, byte who, int fromWho, int tile0, int tile1, int tile2, int tile3)
    {
      _stream.WriteByte((byte) type);
      _stream.WriteByte(who);
      _stream.WriteByte((byte) fromWho);
      _stream.WriteByte((byte) tile0);
      _stream.WriteByte((byte) tile1);
      _stream.WriteByte((byte) tile2);
      _stream.WriteByte((byte) tile3);
    }
  }
}