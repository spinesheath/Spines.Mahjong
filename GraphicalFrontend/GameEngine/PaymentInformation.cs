namespace GraphicalFrontend.GameEngine
{
  internal class PaymentInformation
  {
    // TODO expand payment info
    
    public int Han { get; set; }

    public int Fu { get; set; }

    public int[] ScoreChanges { get; } = new int[4];
  }
}