using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Intrinsics;

namespace Spines.Mahjong.Analysis.Resources
{
  /// <summary>
  /// Parses embedded resources.
  /// </summary>
  internal static class Resource
  {
    public static long[] LongLookup(string category, string resourceName)
    {
      var fullResourceName = ResourceName(category, resourceName);
      var assembly = Assembly.GetExecutingAssembly();
      using var stream = assembly.GetManifestResourceStream(fullResourceName);
      if (stream == null)
      {
        throw new FileNotFoundException($"Resource is missing: {category}/{resourceName}");
      }

      using var reader = new BinaryReader(stream);
      var count = stream.Length / 8;
      var data = new long[count];
      for (var i = 0; i < count; i++)
      {
        data[i] = reader.ReadInt64();
      }

      return data;
    }

    public static byte[] Lookup(string resourceName)
    {
      var fullResourceName = BasePath + resourceName;
      var assembly = Assembly.GetExecutingAssembly();
      using var stream = assembly.GetManifestResourceStream(fullResourceName);
      if (stream == null)
      {
        throw new FileNotFoundException($"Resource is missing: {resourceName}");
      }

      var data = new byte[stream.Length];
      stream.Read(data);
      return data;
    }

    public static byte[] Lookup(string category, string resourceName)
    {
      var fullResourceName = ResourceName(category, resourceName);
      var assembly = Assembly.GetExecutingAssembly();
      using var stream = assembly.GetManifestResourceStream(fullResourceName);
      if (stream == null)
      {
        throw new FileNotFoundException($"Resource is missing: {category}/{resourceName}");
      }

      var data = new byte[stream.Length];
      stream.Read(data);
      return data;
    }

    /// <summary>
    /// Loads the transition table from an embedded resource.
    /// </summary>
    public static ushort[] Transitions(string resourceName)
    {
      var fullResourceName = BasePath + resourceName;
      var assembly = Assembly.GetExecutingAssembly();
      Stream? stream = null;
      try
      {
        stream = assembly.GetManifestResourceStream(fullResourceName);
        if (stream == null)
        {
          throw new FileNotFoundException($"Resource is missing: {resourceName}");
        }

        using var reader = new StreamReader(stream);
        stream = null;
        var result = reader.ReadToEnd();
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        var ints = lines.Select(line => Convert.ToInt32(line, CultureInfo.InvariantCulture));
        return ints.Select(i => i < 0 ? 0 : i).Select(i => (ushort) i).ToArray();
      }
      finally
      {
        stream?.Dispose();
      }
    }

    public static Vector128<byte>[] Vector128Lookup(string category, string resourceName)
    {
      var fullResourceName = ResourceName(category, resourceName);
      var assembly = Assembly.GetExecutingAssembly();
      using var stream = assembly.GetManifestResourceStream(fullResourceName);
      if (stream == null)
      {
        throw new FileNotFoundException($"Resource is missing: {category}/{resourceName}");
      }

      using var reader = new BinaryReader(stream);
      var count = stream.Length / 16;
      var data = new Vector128<byte>[count];
      for (var i = 0; i < count; i++)
      {
        var low = reader.ReadInt64();
        var high = reader.ReadInt64();
        data[i] = Vector128.Create(low, high).AsByte();
      }

      return data;
    }

    private const string BasePath = "Spines.Mahjong.Analysis.Resources.";

    private static string ResourceName(string category, string resourceName)
    {
      return BasePath + category + "." + resourceName;
    }
  }
}