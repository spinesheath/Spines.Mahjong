using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnalyzerBuilder.Classification;
using AnalyzerBuilder.Combinations;

namespace AnalyzerBuilder.Creators
{
  internal class OrderedArrangementsCreator
  {
    public OrderedArrangementsCreator(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    /// <summary>
    /// Creates a file with all arrangements in the order that is used for the alphabet of the classifier.
    /// </summary>
    public IEnumerable<IList<Arrangement>> Create()
    {
      var orderedPath = Path.Combine(_workingDirectory, "OrderedArrangements.txt");
      if (File.Exists(orderedPath))
      {
        return File.ReadAllLines(orderedPath).Select(Arrangement.MultipleFromString).Select(a => a.ToList());
      }

      var words = new ArrangementWordCreator(_workingDirectory).CreateUnordered();
      var classifierBuilder = new ClassifierBuilder();
      classifierBuilder.SetLanguage(words);

      var nullTransitions = CountNullTransitions(classifierBuilder);
      var ordered = nullTransitions.Select((n, i) => new {n, i}).OrderBy(p => p.n).Select(p => p.i).ToList();

      var oldArrangements = new CompactAnalyzedDataCreator(_workingDirectory).GetUniqueArrangements().ToList();
      var newArrangements = ordered.Select(i => oldArrangements[i]).ToList();

      var lines = newArrangements.Select(a => string.Join("", a)).ToList();
      File.WriteAllLines(orderedPath, lines);
      return newArrangements;
    }

    private readonly string _workingDirectory;

    /// <summary>
    /// Counts the number of null transitions per character in the alphabet.
    /// </summary>
    private static IEnumerable<int> CountNullTransitions(IStateMachineBuilder builder)
    {
      var nullTransitions = new int[builder.AlphabetSize];
      for (var i = 0; i < builder.Transitions.Count; i++)
      {
        if (builder.IsResult(i))
        {
          continue;
        }
        if (builder.IsNull(i))
        {
          nullTransitions[i % builder.AlphabetSize] += 1;
        }
      }
      return nullTransitions;
    }
  }
}