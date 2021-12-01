using System;
using System.Collections.Generic;
using Spines.Mahjong.Analysis.Replay;

namespace CompressedReplayCreator
{
  internal sealed class SanmaYonmaBundleWriter : IDisposable
  {
    public SanmaYonmaBundleWriter(string sanmaPath, string yonmaPath, int bundleSize)
    {
      _sanma = new BundleWriter(sanmaPath, bundleSize);
      _yonma = new BundleWriter(yonmaPath, bundleSize);
      _current = _yonma;
    }

    public void Agari(byte who, byte fromWho, byte paoWho, byte[] ba, byte[] hai, IEnumerable<int> scores, IEnumerable<int> ten, byte machi,
      byte[] yaku, byte[] yakuman, byte[] dora, byte[] uraDora, string? meldCodes)
    {
      _current.Agari(who, fromWho, paoWho, ba, hai, scores, ten, machi, yaku, yakuman, dora, uraDora, meldCodes);
    }

    public void CallRiichi(byte who)
    {
      _current.CallRiichi(who);
    }

    public void Discard(byte tileId)
    {
      _current.Discard(tileId);
    }

    public void Dora(byte tileId)
    {
      _current.Dora(tileId);
    }

    public void Draw(byte tileId)
    {
      _current.Draw(tileId);
    }

    public void Go(GameTypeFlag flags)
    {
      if ((flags & GameTypeFlag.Sanma) != 0)
      {
        _current = _sanma;
      }
      else
      {
        _current = _yonma;
      }

      _current.Go(flags);
    }

    public void Init(byte[] seed, IEnumerable<int> ten, byte oya, byte[][] hai)
    {
      _current.Init(seed, ten, oya, hai);
    }

    public void Meld(byte who, string meldCode)
    {
      _current.Meld(who, meldCode);
    }

    public void PayRiichi(byte who)
    {
      _current.PayRiichi(who);
    }

    public void Ryuukyoku(RyuukyokuType type, byte[] ba, IEnumerable<int> scores, bool[] tenpai)
    {
      _current.Ryuukyoku(type, ba, scores, tenpai);
    }

    public void Tsumogiri(byte tileId)
    {
      _current.Tsumogiri(tileId);
    }

    public void Dispose()
    {
      _sanma.Dispose();
      _yonma.Dispose();
    }

    private readonly BundleWriter _sanma;
    private readonly BundleWriter _yonma;
    private BundleWriter _current;
  }
}