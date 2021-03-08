namespace Spines.Mahjong.Analysis.Shanten
{
  /// <summary>
  /// Calculates arrangement value of a suit.
  /// </summary>
  public class SuitClassifier
  {
    public SuitClassifier Clone()
    {
      return new SuitClassifier {_entry = _entry, _meldCount = _meldCount, _secondPhase = _secondPhase};
    }

    public void SetMelds(int melds)
    {
      var current = 0;
      _meldCount = 0;
      for (var i = 0; i < 5; ++i)
      {
        var m = melds & 0b111111;
        if (m != 0)
        {
          current = SuitFirstPhase[current + m];
          melds >>= 6;
          _meldCount += 1;
        }
        else
        {
          break;
        }
      }
      _entry = SuitFirstPhase[current];
      _secondPhase = SuitSecondPhases[_meldCount];
    }

    public int GetValue(byte[] tiles, int suit, int[] base5Hashes)
    {
      var offset = suit * 9;
      switch (_meldCount)
      {
        case 0:
          return SuitBase5Lookup[base5Hashes[suit]];
        case 1:
        {
          var current = _entry;
          current = _secondPhase[current + tiles[offset + 0]];
          current = _secondPhase[current + tiles[offset + 1]];
          current = _secondPhase[current + tiles[offset + 2]];
          current = _secondPhase[current + tiles[offset + 3]] + 11752;
          current = _secondPhase[current + tiles[offset + 4]] + 30650;
          current = _secondPhase[current + tiles[offset + 5]] + 55952;
          current = _secondPhase[current + tiles[offset + 6]] + 80078;
          current = _secondPhase[current + tiles[offset + 7]] + 99750;
          return _secondPhase[current + tiles[offset + 8]];
        }
        case 2:
        {
          var current = _entry;
          current = _secondPhase[current + tiles[offset + 0]];
          current = _secondPhase[current + tiles[offset + 1]];
          current = _secondPhase[current + tiles[offset + 2]] + 22358;
          current = _secondPhase[current + tiles[offset + 3]] + 54162;
          current = _secondPhase[current + tiles[offset + 4]] + 90481;
          current = _secondPhase[current + tiles[offset + 5]] + 120379;
          current = _secondPhase[current + tiles[offset + 6]] + 139662;
          current = _secondPhase[current + tiles[offset + 7]] + 150573;
          return _secondPhase[current + tiles[offset + 8]];
        }
        case 3:
        {
          var current = _entry;
          current = _secondPhase[current + tiles[offset + 0]];
          current = _secondPhase[current + tiles[offset + 1]] + 24641;
          current = _secondPhase[current + tiles[offset + 2]] + 50680;
          current = _secondPhase[current + tiles[offset + 3]] + 76245;
          current = _secondPhase[current + tiles[offset + 4]] + 93468;
          current = _secondPhase[current + tiles[offset + 5]] + 102953;
          current = _secondPhase[current + tiles[offset + 6]] + 107217;
          current = _secondPhase[current + tiles[offset + 7]] + 108982;
          return _secondPhase[current + tiles[offset + 8]];
        }
        case 4:
        {
          var current = _entry;
          current = _secondPhase[current + tiles[offset + 0]];
          current = _secondPhase[current + tiles[offset + 1]];
          current = _secondPhase[current + tiles[offset + 2]];
          current = _secondPhase[current + tiles[offset + 3]];
          current = _secondPhase[current + tiles[offset + 4]];
          current = _secondPhase[current + tiles[offset + 5]];
          current = _secondPhase[current + tiles[offset + 6]];
          current = _secondPhase[current + tiles[offset + 7]];
          return _secondPhase[current + tiles[offset + 8]];
        }
      }

      return 0;
    }

    private ushort[] _secondPhase = SuitSecondPhase0;
    private int _meldCount;
    private int _entry;

    private static readonly ushort[] SuitFirstPhase = Resource.Transitions("SuitFirstPhase.txt");
    private static readonly ushort[] SuitSecondPhase0 = Resource.Transitions("SuitSecondPhase0.txt");
    private static readonly ushort[] SuitSecondPhase1 = Resource.Transitions("SuitSecondPhase1.txt");
    private static readonly ushort[] SuitSecondPhase2 = Resource.Transitions("SuitSecondPhase2.txt");
    private static readonly ushort[] SuitSecondPhase3 = Resource.Transitions("SuitSecondPhase3.txt");
    private static readonly ushort[] SuitSecondPhase4 = Resource.Transitions("SuitSecondPhase4.txt");
    private static readonly byte[] SuitBase5Lookup = Resource.Lookup("suitArrangementsBase5NoMelds.dat");

    private static readonly ushort[][] SuitSecondPhases =
    {
      SuitSecondPhase0,
      SuitSecondPhase1,
      SuitSecondPhase2,
      SuitSecondPhase3,
      SuitSecondPhase4
    };
  }
}