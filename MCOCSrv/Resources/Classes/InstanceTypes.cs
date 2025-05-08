namespace MCOCSrv.Resources.Raw
{
    public enum InstanceType
    {
        Vanilla,
        Forge,
        Neoforge,
        Fabric,
        Quilt,
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
            "Neoforge" => InstanceType.Neoforge,
            "Fabric" => InstanceType.Fabric,
            "Quilt" => InstanceType.Quilt,
            "Paper" => InstanceType.Paper,
            "Purpur" => InstanceType.Purpur,
            "Sponge" => InstanceType.Sponge
        };

        public static string ToString(InstanceType type) => type switch
        {
            InstanceType.Vanilla => "Vanilla",
            InstanceType.Forge => "Forge",
            InstanceType.Neoforge => "Neoforge",
            InstanceType.Fabric => "Fabric",
            InstanceType.Quilt => "Quilt",
            InstanceType.Paper => "Paper",
            InstanceType.Purpur => "Purpur",
            InstanceType.Sponge => "Sponge"
        };
    }

}
