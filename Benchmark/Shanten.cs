using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Spines.Mahjong.Analysis.Replay;

namespace Benchmark
{
  public class Shanten
  {
    private const string ReplaysFolder = @".\..\..\..\..\..\..\..\..\Data\tenhou";
    private readonly List<string> _files;

    public Shanten()
    {
      _files = Directory.EnumerateFiles(ReplaysFolder).Take(10000).ToList();
    }

    private int RunXmlReader()
    {
      return _files.Sum(f => ReplayParser.Parse(System.Xml.XmlReader.Create(f)));
    }

    [Benchmark]
    public int XmlReader() => RunXmlReader();
  }
}