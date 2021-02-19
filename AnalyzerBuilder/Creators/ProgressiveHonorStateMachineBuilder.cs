using System.Collections.Generic;
using System.Linq;
using AnalyzerBuilder.Classification;

namespace AnalyzerBuilder.Creators
{
  internal class ProgressiveHonorStateMachineBuilder : IStateMachineBuilder
  {
    /// <summary>
    /// The size of the alphabet.
    /// </summary>
    public int AlphabetSize => 15 + 1;

    /// <summary>
    /// The transitions for the specified language.
    /// </summary>
    public IReadOnlyList<int> Transitions => _transitions;

    /// <summary>
    /// The states at which the transitions can be entered.
    /// </summary>
    /// <returns>The ids of the states.</returns>
    public IReadOnlyList<int> EntryStates => new[] {0};

    public void SetLanguage(IEnumerable<WordWithValue> language)
    {
      CreateLookupData(language);
      CreateTransitions();
    }

    /// <summary>
    /// Is the transition one that describes can not be reached with a legal word?
    /// </summary>
    /// <param name="transition">The Id of the transition.</param>
    /// <returns>True, if the transition can not be reached, false otherwise.</returns>
    public bool IsNull(int transition)
    {
      return _nullTransitionIds.Contains(transition);
    }

    /// <summary>
    /// Is the transition one that describes a result?
    /// </summary>
    /// <param name="transition">The Id of the transition.</param>
    /// <returns>True, if the transition is a result, false otherwise.</returns>
    public bool IsResult(int transition)
    {
      return transition % AlphabetSize == 0;
    }

    private readonly Dictionary<int, int> _idToValue = new Dictionary<int, int>();
    private readonly Dictionary<int, int> _idToStateColumn = new Dictionary<int, int>();
    private readonly List<int> _stateColumnToId = new List<int>();

    private ISet<int> _nullTransitionIds;
    private int[] _transitions;

    private void CreateTransitions()
    {
      var columnCount = _stateColumnToId.Count;
      _transitions = new int[AlphabetSize * columnCount];
      _nullTransitionIds = new HashSet<int>();

      for (var columnIndex = 0; columnIndex < columnCount; columnIndex++)
      {
        var id = _stateColumnToId[columnIndex];
        _transitions[AlphabetSize * columnIndex] = _idToValue[id]; // value in row 0
        for (var c = 1; c < AlphabetSize; ++c)
        {
          var next = GetNext(id, c - 1);
          var transitionId = AlphabetSize * columnIndex + c;
          if (next.HasValue)
          {
            _transitions[transitionId] = AlphabetSize * _idToStateColumn[next.Value];
          }
          else
          {
            _nullTransitionIds.Add(transitionId);
          }
        }
      }
    }

    private void CreateLookupData(IEnumerable<WordWithValue> language)
    {
      foreach (var word in language)
      {
        var id = GetId(word.Word);
        if (!_idToValue.ContainsKey(id))
        {
          _idToValue.Add(id, word.Value);
        }
      }

      _stateColumnToId.AddRange(_idToValue.Keys.OrderBy(x => x));
      for (var i = 0; i < _stateColumnToId.Count; ++i)
      {
        _idToStateColumn.Add(_stateColumnToId[i], i);
      }
    }

    private static int? GetNext(int s, int c)
    {
      var totalCount = 0;
      var word = new int[15];
      for (var i = 0; i < 7; ++i)
      {
        var t = (s >> i * 3) & 7;
        if (t == 7)
        {
          word[i + 1] = 4;
          totalCount += 3;
        }
        else if (t > 4)
        {
          word[i + 1] = 3;
          totalCount += 3;
          word[i + 7 + 1] = t - 5;
          totalCount += t - 5;
        }
        else
        {
          word[i + 7 + 1] = t;
          totalCount += t;
        }
      }

      if (c <= 3) // draw without meld
      {
        if (totalCount == 14) // cant draw more
        {
          return null;
        }

        var existingCount = c;
        for (var i = 0; i < 7; ++i)
        {
          if (word[i + 7 + 1] == existingCount && word[i + 1] == 0)
          {
            word[i + 7 + 1] += 1;
            break;
          }
          if (i == 6) // hand does not match up with action requirements
          {
            return null;
          }
        }
      }
      else if (c == 4) // draw with meld
      {
        if (totalCount == 14) // cant draw more
        {
          return null;
        }

        for (var i = 0; i < 7; ++i)
        {
          if (word[i + 7 + 1] == 0 && word[i + 1] == 3)
          {
            word[i + 7 + 1] = 1;
            break;
          }
          if (i == 6) // hand does not match up with action requirements
          {
            return null;
          }
        }
      }
      else if (c <= 8) // discard without meld
      {
        var existingCount = c - 4;
        for (var i = 0; i < 7; ++i)
        {
          if (word[i + 7 + 1] == existingCount && word[i + 1] == 0)
          {
            word[i + 7 + 1] -= 1;
            break;
          }
          if (i == 6) // hand does not match up with action requirements
          {
            return null;
          }
        }
      }
      else if (c == 9) // discard with meld
      {
        for (var i = 0; i < 7; ++i)
        {
          if (word[i + 7 + 1] == 1 && word[i + 1] == 3)
          {
            word[i + 7 + 1] = 0;
            break;
          }
          if (i == 6) // hand does not match up with action requirements
          {
            return null;
          }
        }
      }
      else if (c <= 11) // pon
      {
        if (totalCount == 14) // cant pon here
        {
          return null;
        }

        var existingCount = c - 8;
        for (var i = 0; i < 7; ++i)
        {
          if (word[i + 7 + 1] == existingCount && word[i + 1] == 0)
          {
            word[i + 7 + 1] -= 2;
            word[i + 1] = 3;
            break;
          }
          if (i == 6) // hand does not match up with action requirements
          {
            return null;
          }
        }
      }
      else if (c == 12) // daiminkan
      {
        if (totalCount == 14) // cant daiminkan here
        {
          return null;
        }

        for (var i = 0; i < 7; ++i)
        {
          if (word[i + 7 + 1] == 3 && word[i + 1] == 0)
          {
            word[i + 7 + 1] = 0;
            word[i + 1] = 4;
            break;
          }
          if (i == 6) // hand does not match up with action requirements
          {
            return null;
          }
        }
      }
      else if (c == 13) // chakan
      {
        for (var i = 0; i < 7; ++i)
        {
          if (word[i + 7 + 1] == 1 && word[i + 1] == 3)
          {
            word[i + 7 + 1] = 0;
            word[i + 1] = 4;
            break;
          }
          if (i == 6) // hand does not match up with action requirements
          {
            return null;
          }
        }
      }
      else if (c == 14) // ankan
      {
        for (var i = 0; i < 7; ++i)
        {
          if (word[i + 7 + 1] == 4 && word[i + 1] == 0)
          {
            word[i + 7 + 1] = 0;
            word[i + 1] = 4;
            break;
          }
          if (i == 6) // hand does not match up with action requirements
          {
            return null;
          }
        }
      }

      return GetId(word);
    }

    private static int GetId(IReadOnlyList<int> word)
    {
      var c = Enumerable.Range(0, 7).Select(i => word[i + 7 + 1] + word[i + 1] * 2 - word[i + 1] / 3);
      return c.OrderByDescending(x => x).Select((a, i) => a << i * 3).Sum();
    }
  }
}