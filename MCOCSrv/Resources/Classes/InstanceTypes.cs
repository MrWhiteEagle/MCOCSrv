namespace MCOCSrv.Resources.Classes
{
    public enum InstanceType
    {
        Vanilla,
        Forge,
        NeoForge,
        Fabric,
        Paper,
        Purpur,
        Sponge
    }

    public static class InstanceTypes
    {
        public static readonly List<InstanceType> All = Enum.GetValues(typeof(InstanceType)).Cast<InstanceType>().ToList();

        public static InstanceType ToInstanceType(string type) => type switch
        {
            "Vanilla" => InstanceType.Vanilla,
            "Forge" => InstanceType.Forge,
            "NeoForge" => InstanceType.NeoForge,
            "Fabric" => InstanceType.Fabric,
            "Paper" => InstanceType.Paper,
            "Purpur" => InstanceType.Purpur,
            "Sponge" => InstanceType.Sponge,
            _ => InstanceType.Vanilla // Default to Vanilla if unknown type
        };

        public static string ToString(InstanceType type) => type switch
        {
            InstanceType.Vanilla => "Vanilla",
            InstanceType.Forge => "Forge",
            InstanceType.NeoForge => "NeoForge",
            InstanceType.Fabric => "Fabric",
            InstanceType.Paper => "Paper",
            InstanceType.Purpur => "Purpur",
            InstanceType.Sponge => "Sponge",
            _ => "NULL"
        };
    }

}
