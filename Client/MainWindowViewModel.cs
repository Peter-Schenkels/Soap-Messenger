using System.Collections.ObjectModel;

namespace Client
{
    public class MainWindowViewModel
    {
        public ObservableCollection<IMessage> Messages { get; } = new ObservableCollection<IMessage>();

        public MainWindowViewModel()
        {
        }
    }
}
