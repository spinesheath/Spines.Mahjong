using System.Linq;
using System.Threading.Tasks;
using Game.Shared;
using Spines.Mahjong.Analysis.Replay;
using Spines.Mahjong.Analysis.State;

namespace Game.Engine
{
  internal class ExhaustiveDraw : State
  {
    public override State Advance()
    {
      return _nextState!;
    }

    public override Task Decide(Board board, Decider decider)
    {
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

    public override void Update(Board board, Wall wall)
    {
    }

    private State? _nextState;

    private static PaymentInformation CalculateTenpaiPayment(Board board)
    {
      var paymentInformation = new PaymentInformation();

      var tenhouTenpai = board.Seats.Select(s => TenhouShanten.IsTenpai(s.Hand, s.ConcealedTiles, s.Melds.Count)).ToList();
      var tenpaiCount = tenhouTenpai.Count(s => s);
      if (tenpaiCount == 4)
      {
        return paymentInformation;
      }

      for (var i = 0; i < 4; i++)
      {
        if (tenhouTenpai[i])
        {
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