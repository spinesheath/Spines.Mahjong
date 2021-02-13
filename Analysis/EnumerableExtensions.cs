using System;
using System.Collections.Generic;
using System.Linq;

namespace Spines.Mahjong.Analysis
{
  internal static class EnumerableExtensions
  {
    /// <summary>
    /// Returns the first of every segment of stepSize elements of the enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the values in the enumerable.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <param name="stepSize">The length of the segments from which to take the first elements.</param>
    /// <returns>An enumerable with one element per segment.</returns>
    public static IEnumerable<T> Stride<T>(this IEnumerable<T> source, int stepSize)
    {
      return Stride(source, stepSize, 0);
    }

    /// <summary>
    /// Returns the first of every segment of stepSize elements of the enumerable.
    /// </summary>
    /// <typeparam name="T">The type of the values in the enumerable.</typeparam>
    /// <param name="source">The source enumerable.</param>
    /// <param name="stepSize">The length of the segments from which to take the first elements.</param>
    /// <param name="offset">How many elements to skip at the start.</param>
    /// <returns>An enumerable with one element per segment.</returns>
    public static IEnumerable<T> Stride<T>(this IEnumerable<T> source, int stepSize, int offset)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }
      if (stepSize < 1)
      {
        throw new ArgumentOutOfRangeException(nameof(stepSize), "Step size must be greater than 0.");
      }
      if (offset < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(offset), "Offset must not be negative.");
      }

      var current = 0;
      foreach (var value in source.Skip(offset))
      {
        if (current == 0)
        {
          yield return value;
        }
        current += 1;
        if (current == stepSize)
        {
          current = 0;
        }
      }
    }
  }
}
