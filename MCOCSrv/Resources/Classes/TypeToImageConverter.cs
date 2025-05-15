using MCOCSrv.Resources.Raw;
using System.Globalization;

namespace MCOCSrv.Resources.Classes
{
    internal class TypeToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not InstanceType type)
            {
                return "vanilla_icon.png";
            }

            return type switch
            {
                InstanceType.Vanilla => "vanilla_icon.png",
                InstanceType.Forge => "forge_icon.png",
                InstanceType.NeoForge => "neoforge_icon.png",
                InstanceType.Fabric => "fabric_icon.png",
                InstanceType.Paper => "paper_icon.png",
                InstanceType.Purpur => "purpur_icon.png",
                InstanceType.Sponge => "sponge_icon.png",
                _ => "vanilla_icon.png"
            };
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
