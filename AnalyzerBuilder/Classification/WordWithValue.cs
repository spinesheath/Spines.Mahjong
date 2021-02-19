using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace AnalyzerBuilder.Classification
{
  /// <summary>
  /// A word with an associated value.
  /// </summary>
  internal class WordWithValue : IEnumerable<int>
  {
    /// <summary>
    /// Creates a new instance of WordWithValue.
    /// </summary>
    /// <param name="word">The characters of the word.</param>
    /// <param name="value">The value of the word. This is what the word will be classified as.</param>
    public WordWithValue(IEnumerable<int> word, int value)
    {
      Word = word.ToList();
      Value = value;
    }

    /// <summary>
    /// The word.
    /// </summary>
    public IReadOnlyList<int> Word { get; }

    /// <summary>
    /// The value of the Word.
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// The length of the word.
    /// </summary>
    public int Count => Word.Count;

    /// <summary>
    /// Direct access to the characters in the Word Property.
    /// </summary>
    /// <param name="i">The index of the character.</param>
    /// <returns>The character at the given index.</returns>
    public int this[int i] => Word[i];

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An enumerator that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<int> GetEnumerator()
    {
      return Word.GetEnumerator();
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>
    /// A string that represents the current object.
    /// </returns>
    public override string ToString()
    {
      return $"{string.Join(",", Word.Select(c => c.ToString(CultureInfo.InvariantCulture)))}:{Value}";
    }

    /// <summary>
    /// Parses a string and returns a WordWithValue.
    /// </summary>
    /// <param name="word">The string to parse.</param>
    /// <returns>An instance of WordWithValue.</returns>
    public static WordWithValue FromString(string word)
    {
      Debug.Assert(word != null);
      try
      {
        var parts = word.Split(':');
        var parts2 = parts[0].Split(',');
        var characters = parts2.Select(int.Parse);
        return new WordWithValue(characters, int.Parse(parts[1], CultureInfo.InvariantCulture));
      }
      catch (FormatException e)
      {
        throw new FormatException(WordStringFormatError, e);
      }
      catch (IndexOutOfRangeException e)
      {
        throw new FormatException(WordStringFormatError, e);
      }
    }

    private const string WordStringFormatError = "Word must be a comma separated list of integers followed by a colon and another integer.";

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}