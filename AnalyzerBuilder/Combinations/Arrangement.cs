using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace AnalyzerBuilder.Combinations
{
  /// <summary>
  /// Information about an arrangement of tiles relevant for counting shanten.
  /// Only for regular hand shapes of one pair and four mentsu.
  /// </summary>
  internal class Arrangement : IEquatable<Arrangement>
  {
    /// <summary>
    /// Creates a new Arrangement.
    /// </summary>
    public Arrangement(int jantouValue, int mentsuCount, int mentsuValue)
    {
      Debug.Assert(jantouValue >= 0 && jantouValue <= 2);
      Debug.Assert(mentsuCount >= 0 && mentsuCount <= 4);
      Debug.Assert(mentsuValue >= 0 && mentsuValue <= 12);
      Debug.Assert(mentsuValue >= mentsuCount);
      JantouValue = jantouValue;
      MentsuCount = mentsuCount;
      MentsuValue = mentsuValue;
      Id = JantouValue * 25 + MentsuCount * (MentsuCount - 1) + MentsuValue;
    }

    /// <summary>
    /// The value of the pair (if present) in the arrangement (complete or incomplete).
    /// </summary>
    public int JantouValue { get; }

    /// <summary>
    /// The number of mentsu (complete or incomplete).
    /// </summary>
    public int MentsuCount { get; }

    /// <summary>
    /// The number of tiles used in mentsu.
    /// </summary>
    public int MentsuValue { get; }

    /// <summary>
    /// The Id of the arrangement.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// The total value of the arrangement.
    /// </summary>
    public int TotalValue => MentsuValue + JantouValue;

    /// <summary>
    /// Does the arrangement have a jantou?
    /// </summary>
    public bool HasJantou => 0 != JantouValue;

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
    /// </returns>
    /// <param name="other">An object to compare with this object.</param>
    public bool Equals(Arrangement? other)
    {
      if (ReferenceEquals(null, other))
      {
        return false;
      }
      if (ReferenceEquals(this, other))
      {
        return true;
      }
      return Id == other.Id;
    }

    /// <summary>
    /// Creates a new instance of Arrangement with a different jantou value.
    /// </summary>
    public Arrangement SetJantouValue(int value)
    {
      Debug.Assert(value >= 0 && value <= 2);
      return new Arrangement(value, MentsuCount, MentsuValue);
    }

    /// <summary>
    /// Creates a new instance of Arrangement with an added mentsu.
    /// </summary>
    public Arrangement AddMentsu(int value)
    {
      Debug.Assert(value >= 0 && value <= 3);
      return new Arrangement(JantouValue, MentsuCount + 1, MentsuValue + value);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current object.
    /// </summary>
    /// <returns>
    /// true if the specified object is equal to the current object; otherwise, false.
    /// </returns>
    /// <param name="obj">The object to compare with the current object. </param>
    public override bool Equals(object? obj)
    {
      if (ReferenceEquals(null, obj))
      {
        return false;
      }
      if (ReferenceEquals(this, obj))
      {
        return true;
      }
      if (obj.GetType() != GetType())
      {
        return false;
      }
      return Equals((Arrangement) obj);
    }

    /// <summary>
    /// Serves as the default hash function.
    /// </summary>
    /// <returns>
    /// A hash code for the current object.
    /// </returns>
    public override int GetHashCode()
    {
      return Id;
    }

    /// <summary>
    /// Checks if the Arrangements are equal.
    /// </summary>
    public static bool operator ==(Arrangement left, Arrangement? right)
    {
      return Equals(left, right);
    }

    /// <summary>
    /// Checks if the Arrangements are not equal.
    /// </summary>
    public static bool operator !=(Arrangement left, Arrangement? right)
    {
      return !Equals(left, right);
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>
    /// A string that represents the current object.
    /// </returns>
    [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider",
      MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)")]
    public override string ToString()
    {
      return $"{ToChar(JantouValue)}{ToChar(MentsuCount)}{ToChar(MentsuValue)}";
    }

    /// <summary>
    /// Creates an arrangement from a 3 character string.
    /// </summary>
    /// <param name="arrangement">The string to parse.</param>
    /// <returns>An instance of Arrangement.</returns>
    public static Arrangement FromString(string arrangement)
    {
      if (arrangement.Length != 3)
      {
        throw new ArgumentException("s must be exactly three characters long.");
      }
      return new Arrangement(FromChar(arrangement[0]), FromChar(arrangement[1]), FromChar(arrangement[2]));
    }

    /// <summary>
    /// Creates multiple arrangements from a string with 3 characters per arrangement.
    /// </summary>
    /// <param name="arrangements">The string to parse.</param>
    /// <returns>A sequence of arrangements.</returns>
    public static IEnumerable<Arrangement> MultipleFromString(string arrangements)
    {
      for (var i = 0; i < arrangements.Length; i += 3)
      {
        var arrangement = arrangements.Substring(i, 3);
        yield return FromString(arrangement);
      }
    }

    private static char ToChar(int n)
    {
      if (n < 10)
      {
        return (char) ('0' + n);
      }
      return (char) ('A' + n - 10);
    }

    private static int FromChar(char n)
    {
      if (n > '9')
      {
        return n - 'A' + 10;
      }
      return n - '0';
    }
  }
}