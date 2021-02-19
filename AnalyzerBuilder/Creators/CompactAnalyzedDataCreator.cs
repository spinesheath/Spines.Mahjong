using System.Collections.Generic;
using System.IO;
using System.Linq;
using AnalyzerBuilder.Classification;
using AnalyzerBuilder.Combinations;

namespace AnalyzerBuilder.Creators
{
  /// <summary>
  /// Creates data for suits and honors with compact arrangements.
  /// </summary>
  internal class CompactAnalyzedDataCreator
  {
    public CompactAnalyzedDataCreator(string workingDirectory)
    {
      _workingDirectory = workingDirectory;
    }

    public IEnumerable<IList<Arrangement>> GetUniqueArrangements()
    {
      var uniquePath = Path.Combine(_workingDirectory, "UnorderedArrangements.txt");
      if (File.Exists(uniquePath))
      {
        return File.ReadAllLines(uniquePath).Select(a => Arrangement.MultipleFromString(a).ToList());
      }
      var allLines = Create();
      var distinct = allLines.Select(a => a.Substring(HandLength)).Distinct().OrderBy(x => x).ToList();
      File.WriteAllLines(uniquePath, distinct);
      return distinct.Select(a => Arrangement.MultipleFromString(a).ToList());
    }

    public IEnumerable<WordWithValue> CreateHonorWords()
    {
      var files = RawAnalyzedDataCreator.ForHonors().Create(_workingDirectory);
      return CreateWords(files);
    }

    public IEnumerable<WordWithValue> CreateSuitWords()
    {
      var files = RawAnalyzedDataCreator.ForSuits().Create(_workingDirectory);
      return CreateWords(files);
    }

    private const int HandLength = 1 + 9 + 9;
    private readonly string _workingDirectory;

    private IEnumerable<string> Create()
    {
      var honorFiles = RawAnalyzedDataCreator.ForHonors().Create(_workingDirectory);
      var suitFiles = RawAnalyzedDataCreator.ForSuits().Create(_workingDirectory);

      return CreateCompactData(honorFiles).Concat(CreateCompactData(suitFiles));
    }

    private IEnumerable<WordWithValue> CreateWords(IEnumerable<string> files)
    {
      var compact = CreateCompactData(files);
      var orderedArrangements =
        new OrderedArrangementsCreator(_workingDirectory).Create().Select(a => string.Join("", a)).ToList();
      foreach (var analyzedHand in compact)
      {
        var hand = analyzedHand.Substring(0, HandLength).Where(char.IsDigit).Select(c => (int) char.GetNumericValue(c));
        var arrangement = analyzedHand.Substring(HandLength);
        var arrangementId = orderedArrangements.IndexOf(arrangement);
        yield return new WordWithValue(hand, arrangementId);
      }
    }

    private IEnumerable<string> CreateCompactData(IEnumerable<string> fileNames)
    {
      var redundancies = CreateRedundantArrangements(_workingDirectory);
      foreach (var fileName in fileNames)
      {
        var newFileName = fileName.Replace(".txt", "_c.txt");
        if (File.Exists(newFileName))
        {
          foreach (var line in File.ReadAllLines(newFileName))
          {
            yield return line;
          }
        }
        else
        {
          var lines = File.ReadAllLines(fileName);
          var newLines = lines.Select(line => Compact(line, redundancies)).ToList();
          File.WriteAllLines(newFileName, newLines);
          foreach (var line in newLines)
          {
            yield return line;
          }
        }
      }
    }

    private Dictionary<string, string> CreateRedundantArrangements(string workingDirectory)
    {
      var fileName = Path.Combine(workingDirectory, "replacements.txt");
      if (File.Exists(fileName))
      {
        var redundanciesLines = File.ReadAllLines(fileName);
        return redundanciesLines.ToDictionary(
          line => line.Substring(0, line.IndexOf('>')),
          line => line.Substring(line.IndexOf('>') + 1));
      }

      var arrangements = GetAllArrangements().ToList();
      var alphabetSize = arrangements.Count;
      var tilesInArrangements = arrangements.Select(a => a.Max(b => b.TotalValue)).ToList();
      var replacements = new Dictionary<string, string>();

      var foundRedundancy = true;
      while (foundRedundancy)
      {
        foundRedundancy = false;

        for (var i = 0; i < arrangements.Count; ++i)
        {
          // If there is only a single arrangement, it can't have any redundancies.
          // Still need to keep those in the list for their interactions with others.
          var arrangement = arrangements[i];
          if (arrangement.Count < 2)
          {
            continue;
          }

          // Check for each arrangement in the current group if it is redundant.
          for (var j = 0; j < arrangement.Count; ++j)
          {
            var isRedundant = true;
            // Create words for all possible combinations of arrangement groups.
            var language = CreateBaseLanguage(alphabetSize);
            foreach (var word in language)
            {
              // If the current group of arrangements is not part of the word, skip the word.
              if (word.All(c => c != i))
              {
                continue;
              }

              // Pick the arrangements that correspond to the word and sum their tile counts.
              var sumOfTiles = word.Sum(c => tilesInArrangements[c]);
              if (sumOfTiles > 14)
              {
                continue;
              }

              // It's impossible to have less than 5 usable tiles in a hand.
              if (sumOfTiles < 5)
              {
                continue;
              }

              // Calculate the shanten for the word.
              var analyzer = new ArrangementAnalyzer();
              foreach (var character in word)
              {
                analyzer.AddSetOfArrangements(arrangements[character]);
              }
              var shanten = analyzer.CalculateShanten();
              if (shanten >= 9)
              {
                continue;
              }

              // Calculate the shanten with one arrangement from the current arrangement group removed.
              var replacement = arrangement.Where((t, index) => index != j).ToList();
              var analyzer2 = new ArrangementAnalyzer();
              foreach (var character in word)
              {
                analyzer2.AddSetOfArrangements(character == i ? replacement : arrangements[character]);
              }
              var shanten2 = analyzer2.CalculateShanten();

              // If for any word that contains the current arrangement group there is difference in shanten,
              // the arrangement that was removed above is not redundant.
              if (shanten != shanten2)
              {
                isRedundant = false;
                break;
              }
            }

            if (isRedundant)
            {
              var current = string.Join("", arrangements[i]);
              arrangements[i].RemoveAt(j);
              var compacted = string.Join("", arrangements[i]);

              if (!replacements.ContainsKey(current) || replacements[current] != compacted)
              {
                replacements.Add(current, compacted);
              }

              foundRedundancy = true;
              break;
            }
          }
          if (foundRedundancy)
          {
            break;
          }
        }
      }

      var lines = replacements.Select(r => r.Key + ">" + r.Value);
      File.WriteAllLines(fileName, lines);
      return replacements;
    }

    private IEnumerable<List<Arrangement>> GetAllArrangements()
    {
      var honorFiles = RawAnalyzedDataCreator.ForHonors().Create(_workingDirectory);
      var suitFiles = RawAnalyzedDataCreator.ForSuits().Create(_workingDirectory);
      var allLines = honorFiles.Concat(suitFiles).SelectMany(File.ReadAllLines);
      var arrangementStrings = allLines.Select(a => a.Substring(HandLength)).Distinct();
      return arrangementStrings.Select(Arrangement.MultipleFromString).Select(a => a.ToList());
    }

    private static string Compact(string line, IReadOnlyDictionary<string, string> redundancies)
    {
      var arrangements = line.Substring(HandLength);
      while (redundancies.ContainsKey(arrangements))
      {
        arrangements = redundancies[arrangements];
      }
      return line.Substring(0, HandLength) + arrangements;
    }

    /// <summary>
    /// Creates all permutations of length 4 of the numbers 0 through alphabetSize - 1.
    /// </summary>
    /// <param name="alphabetSize">The number of characters in the language.</param>
    /// <returns>The language.</returns>
    private static IEnumerable<IList<int>> CreateBaseLanguage(int alphabetSize)
    {
      for (var a = 0; a < alphabetSize; ++a)
      {
        for (var b = a; b < alphabetSize; ++b)
        {
          for (var c = b; c < alphabetSize; ++c)
          {
            for (var d = c; d < alphabetSize; ++d)
            {
              yield return new[] {a, b, c, d};
            }
          }
        }
      }
    }
  }
}