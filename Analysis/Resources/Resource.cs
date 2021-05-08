﻿using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Spines.Mahjong.Analysis.Resources
{
  /// <summary>
  /// Parses embedded resources.
  /// </summary>
  internal static class Resource
  {
    /// <summary>
    /// Loads the transition table from an embedded resource.
    /// </summary>
    public static ushort[] Transitions(string resourceName)
    {
      var fullResourceName = "Spines.Mahjong.Analysis.Resources." + resourceName;
      var assembly = Assembly.GetExecutingAssembly();
      Stream? stream = null;
      try
      {
        stream = assembly.GetManifestResourceStream(fullResourceName);
        if (stream == null)
        {
          throw new FileNotFoundException("Arrangement classifier transition resource is missing.");
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

    public static byte[] Lookup(string resourceName)
    {
      var fullResourceName = "Spines.Mahjong.Analysis.Resources." + resourceName;
      var assembly = Assembly.GetExecutingAssembly(); 
      using var stream = assembly.GetManifestResourceStream(fullResourceName);
      if (stream == null)
      {
        throw new FileNotFoundException("Arrangement lookup resource is missing.");
      }

      var data = new byte[stream.Length];
      stream.Read(data);
      return data;
    }

    public static long[] LongLookup(string category, string resourceName)
    {
      var fullResourceName = "Spines.Mahjong.Analysis.Resources." + category + "." + resourceName;
      var assembly = Assembly.GetExecutingAssembly();
      using var stream = assembly.GetManifestResourceStream(fullResourceName);
      if (stream == null)
      {
        throw new FileNotFoundException("Arrangement lookup resource is missing.");
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
  }
}