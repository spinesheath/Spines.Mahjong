using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using Spines.Mahjong.Analysis.Replay;

namespace CompressedReplayCreator
{
  /// <summary>
  /// Compresses tenhou replays into a format that can be parsed faster than XML.
  /// </summary>
  class Program
  {
    private static string _sourceDirectory;
    private static string _targetDirectory;
    private static string _sanmaActionsDirectory;
    private static string _yonmaActionsDirectory;
    private static string _sanmaMetadataDirectory;
    private static string _yonmaMetadataDirectory;

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

      _sanmaActionsDirectory = Path.Combine(_targetDirectory, "sanma", "actions");
      if (!Directory.Exists(_sanmaActionsDirectory))
      {
        Directory.CreateDirectory(_sanmaActionsDirectory);
      }

      _yonmaActionsDirectory = Path.Combine(_targetDirectory, "yonma", "actions");
      if (!Directory.Exists(_yonmaActionsDirectory))
      {
        Directory.CreateDirectory(_yonmaActionsDirectory);
      }

      _sanmaMetadataDirectory = Path.Combine(_targetDirectory, "sanma", "meta");
      if (!Directory.Exists(_sanmaMetadataDirectory))
      {
        Directory.CreateDirectory(_sanmaMetadataDirectory);
      }

      _yonmaMetadataDirectory = Path.Combine(_targetDirectory, "yonma", "meta");
      if (!Directory.Exists(_yonmaMetadataDirectory))
      {
        Directory.CreateDirectory(_yonmaMetadataDirectory);
      }

      Convert();
    }

    public static void Convert()
    {
      var count = 0;
      var xmlReaderSettings = new XmlReaderSettings { NameTable = null };
      foreach (var fileName in Directory.EnumerateFiles(_sourceDirectory))
      {
        using var xmlReader = XmlReader.Create(fileName, xmlReaderSettings);
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var actionsFileName = Path.Combine(_targetDirectory, fileNameWithoutExtension + ".actions");
        var metadataFileName = Path.Combine(_targetDirectory, fileNameWithoutExtension + ".meta");

        int playerCount;
        using (var actionsFile = File.Create(actionsFileName))
        using (var metadataFile = File.CreateText(metadataFileName))
        {
          playerCount = ReplayConverter.Compress(xmlReader, actionsFile, metadataFile);
        }

        if (playerCount == 3)
        {
          File.Move(actionsFileName, Path.Combine(_sanmaActionsDirectory, fileNameWithoutExtension + ".actions"));
          File.Move(metadataFileName, Path.Combine(_sanmaMetadataDirectory, fileNameWithoutExtension + ".meta"));
        }
        else if (playerCount == 4)
        {
          File.Move(actionsFileName, Path.Combine(_yonmaActionsDirectory, fileNameWithoutExtension + ".actions"));
          File.Move(metadataFileName, Path.Combine(_yonmaMetadataDirectory, fileNameWithoutExtension + ".meta"));
        }

        count += 1;
        if (count % 1000 == 0)
        {
          Console.WriteLine(count);
        }
      }
    }
  }
}
