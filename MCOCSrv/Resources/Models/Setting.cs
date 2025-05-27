using CommunityToolkit.Mvvm.ComponentModel;

namespace MCOCSrv.Resources.Models
{
    public partial class Setting : ObservableObject
    {
        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _value;

        public Setting(string name, string value)
        {
            this._name = name;
            this._value = value;
        }

    }
}
