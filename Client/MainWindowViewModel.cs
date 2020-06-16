using System.Collections.ObjectModel;

namespace Client
{
    public class MainWindowViewModel
    {
        public ObservableCollection<IMessage> Messages { get; } = new ObservableCollection<IMessage>();

        public MainWindowViewModel()
        {
            Messages.Add(new StringMessage("jrkel"));
            Messages.Add(new CommandMessage("jrkel is een ebwe"));
        }
    }
}
