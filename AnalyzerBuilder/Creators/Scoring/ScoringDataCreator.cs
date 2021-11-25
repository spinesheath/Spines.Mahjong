using System.IO;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal static class ScoringDataCreator
  {
    public static void Create(string directory)
    {
      var footprints = new FootprintCollection();

      var a = new SuitScoringInformationCreator(directory);
      a.CreateLookup(footprints);

      footprints.Serialize(Path.Combine(directory, "SuitFu.dat"));

      var c = new HonorScoringInformationCreator(directory);
      c.CreateLookup();
    }
  }
}
