using System.Linq;
using System.Threading.Tasks;
using Spines.Mahjong.Analysis.Replay;

namespace GraphicalFrontend.GameEngine
{
  internal class ExhaustiveDraw : State
  {
    public override State Advance()
    {
      return _nextState!;
    }

    public override Task Decide(Board board, Decider decider)
    {
      // TODO tenhou apparently doesn't care about calls for tenpai, so tenpai payments are affected by that. Same for riichi (only ankan).
      // In case of riichi, only ankan is relevant, so can just instead check a hand with a minkou (of some unused honor) instead of the ankan
      // In case of ryuukyoku? Can probably just replace all melds with honor triplets too

      _nextState = new EndGame(Enumerable.Empty<int>());

      var anyNagashi = false;
      var calledTiles = board.Seats.SelectMany(s => s.Melds.Where(m => m.MeldType != MeldType.ClosedKan).Select(m => m.CalledTile!)).ToList();
      for (var i = 0; i < 4; i++)
      {
        var seat = board.Seats[i];
        var discards = seat.Discards;
        if (discards.All(t => t.TileType.IsKyuuhai) && !discards.Intersect(calledTiles).Any())
        {
          anyNagashi = true;
          var paymentInformation = CalculateNagashiPayment(board, i);
          _nextState = new Payment(_nextState, paymentInformation);
        }
      }

      if (!anyNagashi)
      {
        _nextState = new Payment(_nextState, CalculateTenpaiPayment(board));
      }

      return Task.CompletedTask;
    }

    public override void Update(Board board)
    {
    }

    private State? _nextState;

    private static PaymentInformation CalculateTenpaiPayment(Board board)
    {
      var paymentInformation = new PaymentInformation();

      var tenpaiCount = board.Seats.Count(s => s.Hand.Shanten == 0);
      for (var i = 0; i < 4; i++)
      {
        if (board.Seats[i].Hand.Shanten == 0)
        {
          tenpaiCount += 1;
          paymentInformation.ScoreChanges[i] = 3000 / tenpaiCount;
        }
        else
        {
          paymentInformation.ScoreChanges[i] = -3000 / (4 - tenpaiCount);
        }
      }

      return paymentInformation;
    }

    private static PaymentInformation CalculateNagashiPayment(Board board, int seatIndex)
    {
      var paymentInformation = new PaymentInformation();
      var isOya = board.Seats[seatIndex].IsOya;
      paymentInformation.ScoreChanges[seatIndex] = isOya ? 12000 : 8000;
      for (var i = 1; i < 4; i++)
      {
        var otherSeatIndex = (seatIndex + 1) % 4;
        var otherIsOya = board.Seats[otherSeatIndex].IsOya;
        paymentInformation.ScoreChanges[otherSeatIndex] = isOya || otherIsOya ? -4000 : -2000;
      }

      return paymentInformation;
    }
  }
}