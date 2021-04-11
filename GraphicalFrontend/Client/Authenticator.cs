using System;
using System.Globalization;

namespace GraphicalFrontend.Client
{
  internal static class Authenticator
  {
    public static string Transform(string authenticationString)
    {
      var parts = authenticationString.Split(AuthenticationStringSplitter);
      if (parts.Length != 2 || parts[0].Length != 8 || parts[1].Length != 8)
      {
        return authenticationString;
      }
      return parts[0] + AuthenticationStringSplitter + CreatePostfix(parts[0], parts[1]);
    }

    private const char AuthenticationStringSplitter = '-';

    private static readonly int[] TranslationTable =
    {
      63006, 9570, 49216, 45888, 9822, 23121, 59830, 51114, 54831, 4189, 580, 5203, 42174, 59972, 55457, 59009, 59347,
      64456, 8673, 52710, 49975, 2006, 62677, 3463, 17754, 5357
    };

    private static string CreatePostfix(string p0, string p1)
    {
      var tableIndex = GetTableIndex(p0);
      var a = TranslationTable[tableIndex] ^ HexToInt32(p1.Substring(0, 4));
      var b = TranslationTable[tableIndex + 1] ^ HexToInt32(p1.Substring(4, 4));
      return ConvertIntToHex4(a) + ConvertIntToHex4(b);
    }

    private static int GetTableIndex(string p0)
    {
      return ToInt("2" + p0.Substring(2, 6)) % (12 - ToInt(p0.Substring(7, 1))) * 2;
    }

    private static string ConvertIntToHex4(int i)
    {
      return ToString(i, "x4");
    }

    private static int ToInt(string s)
    {
      return Convert.ToInt32(s, CultureInfo.InvariantCulture);
    }

    private static string ToString(int i, string format)
    {
      return i.ToString(format, CultureInfo.InvariantCulture);
    }

    private static int HexToInt32(string value)
    {
      return int.Parse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
    }
  }
}
