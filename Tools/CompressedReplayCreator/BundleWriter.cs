using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Spines.Mahjong.Analysis.Replay;

namespace CompressedReplayCreator
{
  internal sealed class BundleWriter : IDisposable
  {
    public BundleWriter(string directory, int bundleSize)
    {
      _directory = directory;
      _bundleSize = bundleSize;

      _stream = File.Create(Path.Combine(_directory, "0.bundle"));
    }

    public void Agari(byte who, byte fromWho, byte paoWho, byte[] ba, byte[] hai, IEnumerable<int> scores, IEnumerable<int> ten, byte machi,
      byte[] yaku, byte[] yakuman, byte[] dora, byte[] uraDora, string? meldCodes)
    {
      var meldCount = meldCodes?.Split(",").Length ?? 0;
      _indexInBlock += 1 + // nodeId
                       1 + // honba
                       1 + // riichi
                       1 + hai.Length + // hai
                       1 + meldCount * 7 + // melds
                       1 + // machi
                       3 * 4 + // ten
                       1 + yaku.Length + // yaku
                       1 + yakuman.Length + // yakuman
                       1 + dora.Length + // dora
                       1 + uraDora.Length + // uraDora
                       1 + // who
                       1 + // fromWho
                       1 + // paoWho
                       2 * 4 * 4; // score

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
          MeldInternal(who, meldCode);
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

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void CallRiichi(byte who)
    {
      _indexInBlock += 2;

      _stream.WriteByte((byte) Node.CallRiichi);
      _stream.WriteByte(who);

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void Discard(byte tileId)
    {
      _indexInBlock += 2;

      _stream.WriteByte((byte) Node.Discard);
      _stream.WriteByte(tileId);

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void Dora(byte tileId)
    {
      _indexInBlock += 2;

      _stream.WriteByte((byte) Node.Dora);
      _stream.WriteByte(tileId);

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void Draw(byte tileId)
    {
      _indexInBlock += 2;

      _stream.WriteByte((byte) Node.Draw);
      _stream.WriteByte(tileId);

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void Go(GameTypeFlag flags)
    {
      StartNewReplay();

      _indexInBlock += 2;

      _stream.WriteByte((byte) Node.Go);
      _stream.WriteByte((byte) flags);

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void Init(byte[] seed, IEnumerable<int> ten, byte oya, byte[][] hai)
    {
      // seed: 6 bytes, ten: 4*4 bytes, oya: 1 byte
      _indexInBlock += 1 + 6 + 4 * 4 + 1;

      _stream.WriteByte((byte) Node.Init);
      _stream.Write(seed);
      _stream.Write(ten.SelectMany(BitConverter.GetBytes).ToArray());
      _stream.WriteByte(oya);

      for (var i = 0; i < 4; i++)
      {
        // 1 byte id, 1 byte playerId, 13 bytes tileIds
        _indexInBlock += 1 + 1 + 13;

        _stream.WriteByte((byte) Node.Haipai);
        _stream.WriteByte((byte) i);
        if (hai[i].Length == 0)
        {
          Debug.Assert(i == 3);
          _stream.Write(new byte[13]);
        }
        else
        {
          Debug.Assert(hai[i].Length == 13);
          _stream.Write(hai[i]);
        }
      }

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void Meld(byte who, string meldCode)
    {
      _indexInBlock += 7;

      MeldInternal(who, meldCode);

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void PayRiichi(byte who)
    {
      _indexInBlock += 2;

      _stream.WriteByte((byte) Node.PayRiichi);
      _stream.WriteByte(who);

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void Ryuukyoku(RyuukyokuType type, byte[] ba, IEnumerable<int> scores, bool[] tenpai)
    {
      //1 byte id, 2 byte ba, 2*4*4 byte score, 1 byte ryuukyokuType, 4 byte tenpaiState
      _indexInBlock += 1 + 2 + 8 * 4 + 1 + 4;

      _stream.WriteByte((byte) Node.Ryuukyoku);

      _stream.Write(ba);
      _stream.Write(scores.SelectMany(BitConverter.GetBytes).ToArray());

      _stream.WriteByte((byte) type.Id);

      for (var i = 0; i < 4; i++)
      {
        _stream.WriteByte(tenpai[i] ? (byte) 0 : (byte) 1);
      }

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void Tsumogiri(byte tileId)
    {
      _indexInBlock += 2;

      _stream.WriteByte((byte) Node.Tsumogiri);
      _stream.WriteByte(tileId);

      Debug.Assert(_indexInBlock == _stream.Length % 1024);
    }

    public void Dispose()
    {
      _stream.Dispose();
    }
    
    private readonly int _bundleSize;
    private readonly string _directory;
    private int _bundleIndex;
    private int _indexInBlock;
    private int _replayInBundleCount;
    private Stream _stream;

    private void StartNewReplay()
    {
      if (_replayInBundleCount == _bundleSize)
      {
        _bundleIndex += 1;
        _stream.Dispose();
        _stream = File.Create(Path.Combine(_directory, $"{_bundleIndex}.bundle"));
        _indexInBlock = 0;
        _replayInBundleCount = 0;
      }
      else
      {
        _indexInBlock += 1;
        _stream.WriteByte((byte) Node.NextReplay);
      }

      _replayInBundleCount += 1;
    }

    private void MeldInternal(byte who, string meldCode)
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