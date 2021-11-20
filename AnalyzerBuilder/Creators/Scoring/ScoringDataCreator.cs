using System;
using System.Collections.Generic;
using System.IO;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class FootprintCollection
  {
    private readonly Dictionary<byte[], int> _footprints = new Dictionary<byte[], int>(new FuConstraintFootprintComparer());

    public int IndexOf(byte[] footprint)
    {
      if (!_footprints.TryGetValue(footprint, out var footprintIndex))
      {
        footprintIndex = _footprints.Count;
        _footprints.Add(footprint, footprintIndex);
      }

      return footprintIndex;
    }

    public void Serialize(string path)
    {
      var allFootprints = new byte[_footprints.Count * 680];
      foreach (var (footprint, index) in _footprints)
      {
        Array.Copy(footprint, 0, allFootprints, index * footprint.Length, footprint.Length);
      }

      using var fileStream = File.Create(path);
      using var writer = new BinaryWriter(fileStream);
      for (var i = 0; i < allFootprints.Length; i++)
      {
        writer.Write(allFootprints[i]);
      }
    }
  }

  internal static class ScoringDataCreator
  {
    public static void Create(string workingDirectory)
    {
      var footprints = new FootprintCollection();

      var a = new SuitScoringInformationCreator(workingDirectory);
      a.CreateLookup(footprints);

      footprints.Serialize(Path.Combine(workingDirectory, "SuitFu.dat"));

      var c = new HonorScoringInformationCreator(workingDirectory);
      c.CreateLookup();
    }
  }
}
