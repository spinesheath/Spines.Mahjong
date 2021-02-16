using System;
using System.IO;

namespace ReplayBundleCreator
{
  class Program
  {
    private static string _sourceDirectory;
    private static string _targetDirectory;

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

      if (!Directory.Exists(_targetDirectory))
      {
        Directory.CreateDirectory(_targetDirectory);
      }

      var i = 0;
      var j = 0;
      FileStream targetFile = null;

      try
      {
        foreach (var fileName in Directory.EnumerateFiles(_sourceDirectory, "*.actions"))
        {
          if (i % 1000 == 0)
          {
            targetFile?.Flush();
            targetFile?.Dispose();
            Console.WriteLine(i);
            targetFile = File.Create(Path.Combine(_targetDirectory, $"{j}.bundle"));
            j += 1;
          }

          var source = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 4096, FileOptions.SequentialScan);
          source.CopyTo(targetFile);
          targetFile.WriteByte(127);

          i += 1;
        }
      }
      finally
      {
        targetFile.Flush();
        targetFile.Dispose();
      }
    }
  }
}
