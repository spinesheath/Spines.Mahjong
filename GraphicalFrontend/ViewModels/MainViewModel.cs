namespace GraphicalFrontend.ViewModels
{
  internal class MainViewModel
  {
    public MainViewModel(MessagesViewModel messagesViewModel, BoardViewModel boardViewModel)
    {
      Messages = messagesViewModel;
      Board = boardViewModel;
    }

    public MessagesViewModel Messages { get; }

    public BoardViewModel Board { get; }
  }
}