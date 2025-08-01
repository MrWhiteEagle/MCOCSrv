using CommunityToolkit.Mvvm.ComponentModel;

namespace MCOCSrv.Resources.Models
{
    public partial class QuickAction : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private string command;

        public QuickAction(string name, string command)
        {
            this.Name = name;
            this.Command = command;
        }
    }
}
