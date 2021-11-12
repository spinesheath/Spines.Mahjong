using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace AnalyzerBuilder.Creators.Scoring
{
  internal class FuConstraintFootprintComparer : IEqualityComparer<byte[]>
  {
    public bool Equals([AllowNull] byte[] x, [AllowNull] byte[] y)
    {
      if (x == y)
      {
        return true;
      }

      if (x == null || y == null)
      {
        return false;
      }

      if (x.Length != y.Length)
      {
        return false;
      }

      return x.SequenceEqual(y);
    }

    public int GetHashCode(byte[] array)
    {
      var hc = array.Length;
      foreach (int val in array)
      {
        hc = unchecked(hc * 31 + val);
      }

      return hc;
    }
  }
}