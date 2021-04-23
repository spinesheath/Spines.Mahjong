using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphicalFrontend.GameEngine
{
  internal class Ron : State
  {
    private readonly IEnumerable<int> _seatIndexes;
    private State? _nextState;

    public Ron(IEnumerable<int> seatIndexes)
    {
      _seatIndexes = seatIndexes.ToList();
    }

    public override void Update(Board board)
    {
    }

    public override Task Decide(Board board, Decider decider)
    {
      _nextState = new EndGame(_seatIndexes);
      foreach (var seatIndex in _seatIndexes)
      {
        var paymentInformation = new PaymentInformation();
        // TODO calculate ron score (include honba and riichi sticks)
        _nextState = new Payment(_nextState, paymentInformation);
      }

      return Task.CompletedTask;
    }

    public override State Advance()
    {
      return _nextState!;
    }
  }
}