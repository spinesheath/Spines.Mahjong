using System;
using System.IO;
using System.Xml;

namespace CompressedReplayCreator
{
  /// <summary>
  /// Compresses tenhou replays into a format that can be parsed faster than XML.
  /// </summary>
  class Program
  {
    private static string _sourceDirectory = "";
    private static string _targetDirectory = "";
    private static string _sanmaDirectory = "";
    private static string _yonmaDirectory = "";

    static void Main(string[] args)
    {
      if (args.Length != 2)
      {
        Console.WriteLine("args: source target");
        Console.ReadKey();
        return;
      }

      _sourceDirectory = args[0];
      _targetDirectory = args[1];

      if (!Directory.Exists(_sourceDirectory))
      {
        Console.WriteLine("source does not exist");
        Console.ReadKey();
        return;
      }

      _sanmaDirectory = Path.Combine(_targetDirectory, "sanma");
      if (!Directory.Exists(_sanmaDirectory))
      {
        Directory.CreateDirectory(_sanmaDirectory);
      }

      _yonmaDirectory = Path.Combine(_targetDirectory, "yonma");
      if (!Directory.Exists(_yonmaDirectory))
      {
        Directory.CreateDirectory(_yonmaDirectory);
      }

      Convert();
    }

    public static void Convert()
    {
      using var bundleWriter = new SanmaYonmaBundleWriter(_sanmaDirectory, _yonmaDirectory, 1000);

      var count = 0;
      var xmlReaderSettings = new XmlReaderSettings { NameTable = null };
      foreach (var fileName in Directory.EnumerateFiles(_sourceDirectory))
      {
        using var xmlReader = XmlReader.Create(fileName, xmlReaderSettings);
        ReplayConverter.Compress(xmlReader, bundleWriter);

        count += 1;
        if (count % 1000 == 0)
        {
          Console.WriteLine(count);
        }
      }
    }
  }
}
